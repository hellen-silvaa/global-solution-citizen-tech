using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Infrastructure;
using Oportuniza.Infrastructure.External.ViaCep;
using Oportuniza.Infrastructure.HealthChecks;
using Oportuniza.Infrastructure.Persistence;
using Oportuniza.Infrastructure.Security;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Infrastructure;

public class ViaCepClientTests
{
    private static ViaCepClient Cliente(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new HttpClient(new HandlerFalso(responder)) { BaseAddress = new Uri("https://viacep.teste/ws/") },
            NullLogger<ViaCepClient>.Instance);

    private static HttpResponseMessage Json(string corpo, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json") };

    [Fact]
    public async Task CepExistente_MapeiaOEndereco()
    {
        Uri? chamada = null;
        var cliente = Cliente(req =>
        {
            chamada = req.RequestUri;
            return Json("""{"cep":"01001-000","logradouro":"Praça da Sé","bairro":"Sé","localidade":"São Paulo","uf":"SP"}""");
        });

        var endereco = await cliente.ConsultarAsync("01001000");

        Assert.Equal("https://viacep.teste/ws/01001000/json/", chamada!.ToString());
        Assert.Equal("01001000", endereco.Cep);
        Assert.Equal("Sé", endereco.Bairro);
        Assert.Equal("São Paulo", endereco.Cidade);
        Assert.Null(endereco.Numero);
    }

    [Fact]
    public async Task CampoNuloNaResposta_ViraTextoVazio()
    {
        var endereco = await Cliente(_ => Json("""{"cep":"01001-000"}""")).ConsultarAsync("01001000");
        Assert.Equal("", endereco.Logradouro);
    }

    [Theory]
    [InlineData("""{"erro":true}""")]
    [InlineData("""{"erro":"true"}""")]
    [InlineData("""{}""")]
    [InlineData("null")]
    public async Task CepInexistente_404(string corpo) =>
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Cliente(_ => Json(corpo)).ConsultarAsync("99999999"));

    [Fact]
    public async Task Resposta400DoViaCep_404() =>
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            Cliente(_ => Json("", HttpStatusCode.BadRequest)).ConsultarAsync("00000000"));

    [Fact]
    public async Task Resposta5xx_503() =>
        await Assert.ThrowsAsync<IntegracaoExternaException>(() =>
            Cliente(_ => Json("", HttpStatusCode.BadGateway)).ConsultarAsync("01001000"));

    [Fact]
    public async Task JsonQuebrado_503() =>
        await Assert.ThrowsAsync<IntegracaoExternaException>(() => Cliente(_ => Json("{quebrado")).ConsultarAsync("01001000"));

    [Fact]
    public async Task FalhaDeRede_503() =>
        await Assert.ThrowsAsync<IntegracaoExternaException>(() =>
            Cliente(_ => throw new HttpRequestException("sem rede")).ConsultarAsync("01001000"));

    [Fact]
    public async Task Timeout_503() =>
        await Assert.ThrowsAsync<IntegracaoExternaException>(() =>
            Cliente(_ => throw new TaskCanceledException("timeout")).ConsultarAsync("01001000"));

    private sealed class HandlerFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(responder(request));
    }
}

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Opcoes = new() { Secret = new string('s', 40), ExpiracaoHoras = 2 };

    [Fact]
    public void Gerar_TokenAssinadoComClaimsEExpiracao()
    {
        var cidadao = Fabrica.Cidadao(10);

        var token = new JwtTokenService(Options.Create(Opcoes), new RelogioFixo(Fabrica.Agora)).Gerar(cidadao);

        var principal = new JwtSecurityTokenHandler().ValidateToken(token.Token, new TokenValidationParameters
        {
            ValidIssuer = Opcoes.Issuer,
            ValidAudience = Opcoes.Audience,
            IssuerSigningKey = Opcoes.ChaveDeAssinatura(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = false
        }, out var validado);

        Assert.Equal("Bearer", token.Tipo);
        Assert.Equal(Fabrica.Agora.AddHours(2), token.ExpiraEm);
        Assert.Equal(Perfil.Cidadao, token.Perfil);
        Assert.Equal("10", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Cidadao", principal.FindFirstValue(ClaimTypes.Role));
        Assert.Equal(SecurityAlgorithms.HmacSha256, ((JwtSecurityToken)validado).Header.Alg);
        Assert.DoesNotContain(principal.Claims, c => c.Value.Contains("hash"));
    }

    [Fact]
    public void Gerar_CadaTokenTemJtiUnico()
    {
        var servico = new JwtTokenService(Options.Create(Opcoes), new RelogioFixo(Fabrica.Agora));
        var jti = (string t) => new JwtSecurityTokenHandler().ReadJwtToken(t).Id;

        Assert.NotEqual(jti(servico.Gerar(Fabrica.Admin()).Token), jti(servico.Gerar(Fabrica.Admin()).Token));
    }

    [Fact]
    public void TokenAssinadoComOutraChave_EInvalido()
    {
        var token = new JwtTokenService(Options.Create(Opcoes), new RelogioFixo(Fabrica.Agora)).Gerar(Fabrica.Admin());

        Assert.ThrowsAny<SecurityTokenException>(() => new JwtSecurityTokenHandler().ValidateToken(token.Token,
            new TokenValidationParameters
            {
                ValidIssuer = Opcoes.Issuer, ValidAudience = Opcoes.Audience, ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(new byte[40])
            }, out _));
    }
}

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_NaoGuardaASenhaESalgaCadaHash()
    {
        var hash1 = _hasher.Hash("Senha@123");
        var hash2 = _hasher.Hash("Senha@123");

        Assert.DoesNotContain("Senha@123", hash1);
        Assert.NotEqual(hash1, hash2);
        Assert.StartsWith("$2", hash1);
    }

    [Fact]
    public void Verificar_SenhaCertaEErrada()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.True(_hasher.Verificar("Senha@123", hash));
        Assert.False(_hasher.Verificar("senha@123", hash));
    }

    [Fact]
    public void Verificar_SemHash_SempreFalso() => Assert.False(_hasher.Verificar("qualquer", null));
}

public class BancoDeDadosHealthCheckTests
{
    [Fact]
    public async Task BancoAcessivel_Healthy()
    {
        using var banco = new BancoEmMemoria();
        using var db = banco.NovoContexto();

        var resultado = await new BancoDeDadosHealthCheck(db).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, resultado.Status);
    }

    [Fact]
    public async Task BancoInacessivel_Unhealthy()
    {
        var caminho = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nao-existe.db");
        using var db = new OportunizaDbContext(new DbContextOptionsBuilder<OportunizaDbContext>()
            .UseSqlite($"Data Source={caminho};Mode=ReadOnly").Options);

        var resultado = await new BancoDeDadosHealthCheck(db).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
    }

    [Fact]
    public async Task ErroAoConectar_Unhealthy()
    {
        using var banco = new BancoEmMemoria();
        var db = banco.NovoContexto();
        await db.DisposeAsync();

        var resultado = await new BancoDeDadosHealthCheck(db).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
        Assert.NotNull(resultado.Exception);
    }
}

public class InfrastructureConfiguracaoTests
{
    private static IConfiguration Configuracao(string? conexao, string segredo) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = conexao,
            ["Jwt:Secret"] = segredo,
            ["ViaCep:BaseUrl"] = "https://viacep.teste/ws"
        })
        .Build();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SemConnectionString_FalhaNaSubida(string? conexao) =>
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddInfrastructure(Configuracao(conexao, new string('s', 40))));

    [Fact]
    public void SegredoJwtCurto_FalhaNaValidacao()
    {
        using var provider = new ServiceCollection().AddInfrastructure(Configuracao("Server=x;Database=y;", "curto")).BuildServiceProvider();

        var erro = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
        Assert.Contains("Jwt__Secret", erro.Message);
    }

    [Fact]
    public void ConfiguracaoValida_ResolveAdaptadores()
    {
        var services = new ServiceCollection().AddLogging().AddSingleton(TimeProvider.System);
        services.AddInfrastructure(Configuracao("Server=x;Database=y;", new string('s', 40)));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<BCryptPasswordHasher>(scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
        Assert.IsType<JwtTokenService>(scope.ServiceProvider.GetRequiredService<ITokenService>());
        Assert.IsType<ViaCepClient>(scope.ServiceProvider.GetRequiredService<IConsultaCepGateway>());
    }

    [Fact]
    public void DesignTimeFactory_UsaVariavelDeAmbienteOuPadraoDeDesenvolvimento()
    {
        var anterior = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Server=outro;Database=teste;");
            using (var db = new DesignTimeDbContextFactory().CreateDbContext([]))
                Assert.Contains("outro", db.Database.GetConnectionString());

            Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
            using (var db = new DesignTimeDbContextFactory().CreateDbContext([]))
                Assert.Contains("localhost", db.Database.GetConnectionString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Default", anterior);
        }
    }

    [Fact]
    public async Task AplicarMigracoes_ExecutaMigrateNoContexto()
    {
        // As migrações foram geradas para MySQL; o SQLite as executa (SQL padrão), validando a sequência
        // completa + o seed. O aviso de "modelo pendente" é só diferença de provider e é ignorado aqui;
        // no MySQL, `dotnet ef migrations has-pending-model-changes` confirma que não há nada pendente.
        using var conexao = new SqliteConnection("DataSource=:memory:");
        conexao.Open();
        var services = new ServiceCollection();
        services.AddDbContext<OportunizaDbContext>(o => o.UseSqlite(conexao)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
        await using var provider = services.BuildServiceProvider();

        await provider.AplicarMigracoesAsync();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OportunizaDbContext>();
        Assert.Equal(db.Database.GetMigrations(), await db.Database.GetAppliedMigrationsAsync());
        Assert.Equal(7, await db.Usuarios.CountAsync()); // seed de dados fictícios aplicado
    }
}
