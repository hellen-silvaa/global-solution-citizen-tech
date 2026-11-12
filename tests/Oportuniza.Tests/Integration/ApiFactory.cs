using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;
using Oportuniza.Infrastructure.External.ViaCep;
using Oportuniza.Infrastructure.Persistence;

namespace Oportuniza.Tests.Integration;

/// <summary>
/// Sobe a API inteira em memória: o mesmo Program.cs, com SQLite in-memory no lugar do MySQL
/// e um ViaCEP falso. Os testes de integração não precisam de banco instalado nem de internet.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string SegredoJwt = "segredo-dos-testes-de-integracao-com-mais-de-32-caracteres";
    public const string SenhaPadrao = "Senha@123";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SqliteConnection _conexao = new("DataSource=:memory:");

    /// <summary>Configurações extras de cada variação da fábrica.</summary>
    protected virtual IDictionary<string, string?> Configuracoes => new Dictionary<string, string?>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var padrao = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Server=nao-usado;Database=nao-usado;",
            ["Database:AplicarMigracoesAoIniciar"] = "false",
            ["Jwt:Secret"] = SegredoJwt,
            ["Seguranca:ForcarHttps"] = "false",
            ["RateLimit:RequisicoesPorMinuto"] = "100000",
            ["RateLimit:LoginPorMinuto"] = "100000"
        };
        foreach (var (chave, valor) in padrao.Concat(Configuracoes).GroupBy(p => p.Key).Select(g => g.Last()))
            builder.UseSetting(chave, valor);

        builder.ConfigureServices(services =>
        {
            // Troca o provider MySQL pelo SQLite em memória.
            foreach (var registro in services.Where(d =>
                         d.ServiceType == typeof(DbContextOptions<OportunizaDbContext>) ||
                         d.ServiceType == typeof(IDbContextOptionsConfiguration<OportunizaDbContext>)).ToList())
                services.Remove(registro);

            _conexao.Open();
            services.AddDbContext<OportunizaDbContext>(o => o.UseSqlite(_conexao)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddHttpClient<IConsultaCepGateway, ViaCepClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new ViaCepFalso());
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<OportunizaDbContext>().Database.EnsureCreated();
    }

    /// <summary>Cria um administrador direto no banco (não existe endpoint público para isso).</summary>
    public async Task<string> CriarAdminETokenAsync(HttpClient http)
    {
        var email = $"admin{Guid.NewGuid():N}@teste.local";
        using (var scope = Services.CreateScope())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var db = scope.ServiceProvider.GetRequiredService<OportunizaDbContext>();
            db.Add(new Administrador("Admin de Teste", Email.Criar(email), hasher.Hash(SenhaPadrao)));
            await db.SaveChangesAsync();
        }
        return await http.LoginAsync(email);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexao.Dispose();
    }

    /// <summary>Simula o ViaCEP: 01001000 existe, 99999999 não existe, 00000000 está fora do ar.</summary>
    private sealed class ViaCepFalso : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("00000000"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            var corpo = url.Contains("01001000")
                ? """{"cep":"01001-000","logradouro":"Praça da Sé","bairro":"Sé","localidade":"São Paulo","uf":"SP"}"""
                : """{"erro":"true"}""";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}

/// <summary>Limite de login baixo, para testar o rate limiting.</summary>
public class ApiFactoryComLimiteDeLogin : ApiFactory
{
    protected override IDictionary<string, string?> Configuracoes =>
        new Dictionary<string, string?> { ["RateLimit:LoginPorMinuto"] = "3" };
}

/// <summary>Produção "endurecida": HTTPS obrigatório e Swagger desligado.</summary>
public class ApiFactoryProducao : ApiFactory
{
    protected override IDictionary<string, string?> Configuracoes => new Dictionary<string, string?>
    {
        ["Seguranca:ForcarHttps"] = "true",
        ["https_port"] = "443",
        ["Swagger:Habilitado"] = "false"
    };
}

/// <summary>Aplica as migrações na subida (como no ambiente de desenvolvimento), incluindo o seed.</summary>
public class ApiFactoryComMigracoes : ApiFactory
{
    protected override IDictionary<string, string?> Configuracoes =>
        new Dictionary<string, string?> { ["Database:AplicarMigracoesAoIniciar"] = "true" };
}

/// <summary>Atalhos para os testes: cadastrar usuários, autenticar e enviar requisições.</summary>
public static class HttpClientExtensions
{
    private static int _contador;

    public static object Endereco(string bairro) =>
        new { cep = "05863-000", logradouro = "Rua Teste", numero = "1", bairro, cidade = "São Paulo", uf = "SP" };

    public static object NovoCidadao(string email, string bairro = "Capão Redondo", string senha = ApiFactory.SenhaPadrao) => new
    {
        nome = "Cidadã de Teste", email, senha, dataNascimento = "2000-01-01",
        escolaridade = "MedioCompleto", areasInteresse = new[] { "Tecnologia" }, endereco = Endereco(bairro)
    };

    public static string NovoEmail(string prefixo = "usuario") => $"{prefixo}{Interlocked.Increment(ref _contador)}@teste.local";

    public static async Task<(long Id, string Token)> CriarCidadaoAsync(this HttpClient http, string bairro = "Capão Redondo")
    {
        var email = NovoEmail("cidadao");
        var resposta = await http.PostAsJsonAsync("/cidadaos", NovoCidadao(email, bairro), ApiFactory.Json);
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return (corpo.GetProperty("id").GetInt64(), await http.LoginAsync(email));
    }

    public static async Task<(long Id, string Token)> CriarOrganizacaoAsync(this HttpClient http)
    {
        var email = NovoEmail("org");
        var resposta = await http.PostAsJsonAsync("/organizacoes", new
        {
            nome = "ONG de Teste", email, senha = ApiFactory.SenhaPadrao, tipo = "Ong",
            descricao = "Organização de teste", site = "https://ong.teste.local", endereco = Endereco("Capão Redondo")
        }, ApiFactory.Json);
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return (corpo.GetProperty("id").GetInt64(), await http.LoginAsync(email));
    }

    public static async Task<string> LoginAsync(this HttpClient http, string email)
    {
        var resposta = await http.PostAsJsonAsync("/auth/login", new { email, senha = ApiFactory.SenhaPadrao });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
    }

    public static object NovaOportunidade(int vagas = 5, string area = "Tecnologia", string titulo = "Curso de teste", int diasParaInicio = 10) => new
    {
        titulo, descricao = "Descrição", tipo = "Curso", modalidade = "Online", area, bairro = "Capão Redondo",
        cargaHoraria = 20, vagas, dataInicio = DateTime.UtcNow.AddDays(diasParaInicio), dataFim = DateTime.UtcNow.AddDays(diasParaInicio + 30)
    };

    public static async Task<long> CriarOportunidadeAsync(this HttpClient http, string tokenOrganizacao, int vagas = 5, string area = "Tecnologia")
    {
        var resposta = await http.EnviarAsync(HttpMethod.Post, "/oportunidades", tokenOrganizacao, NovaOportunidade(vagas, area));
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt64();
    }

    public static async Task<long> InscreverAsync(this HttpClient http, long oportunidadeId, string tokenCidadao)
    {
        var resposta = await http.EnviarAsync(HttpMethod.Post, $"/oportunidades/{oportunidadeId}/inscricoes", tokenCidadao);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt64();
    }

    public static Task<HttpResponseMessage> EnviarAsync(this HttpClient http, HttpMethod metodo, string url, string? token, object? corpo = null)
    {
        var request = new HttpRequestMessage(metodo, url);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (corpo is not null)
            request.Content = JsonContent.Create(corpo, options: ApiFactory.Json);
        return http.SendAsync(request);
    }

    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();
}
