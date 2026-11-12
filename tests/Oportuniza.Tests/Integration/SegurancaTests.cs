using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Infrastructure.Security;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Integration;

/// <summary>Ataques e abusos comuns (OWASP API Top 10) contra a API rodando de verdade.</summary>
public class SegurancaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _http = factory.CreateClient();

    private static string TokenForjado(string chave, string issuer = "Oportuniza.Api", DateTime? expira = null, string perfil = "Admin")
    {
        var token = new JwtSecurityToken(issuer, "Oportuniza",
            [new Claim(JwtRegisteredClaimNames.Sub, "1"), new Claim(ClaimTypes.Role, perfil)],
            notBefore: (expira ?? DateTime.UtcNow.AddHours(1)).AddHours(-2), expires: expira ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ---------- Autenticação (API2: Broken Authentication) ----------

    [Fact]
    public async Task SemToken_401ComMensagemPadrao()
    {
        var resposta = await _http.GetAsync("/cidadaos/1");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal("Token ausente, inválido ou expirado!", (await resposta.JsonAsync()).GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task TokenAssinadoComOutraChave_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", TokenForjado("chave-do-atacante-com-mais-de-32-caracteres!!"))).StatusCode);

    [Fact]
    public async Task TokenDeOutroEmissor_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", TokenForjado(ApiFactory.SegredoJwt, issuer: "atacante"))).StatusCode);

    [Fact]
    public async Task TokenExpirado_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", TokenForjado(ApiFactory.SegredoJwt, expira: DateTime.UtcNow.AddMinutes(-5)))).StatusCode);

    [Fact]
    public async Task TokenSemAssinatura_AlgNone_401()
    {
        var cabecalho = Base64Url("""{"alg":"none","typ":"JWT"}""");
        var corpo = Base64Url($$"""{"sub":"1","http://schemas.microsoft.com/ws/2008/06/identity/claims/role":"Admin","iss":"Oportuniza.Api","aud":"Oportuniza","exp":{{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}""");

        Assert.Equal(HttpStatusCode.Unauthorized, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", $"{cabecalho}.{corpo}.")).StatusCode);
    }

    [Fact]
    public async Task TokenAdulterado_TrocandoOPerfil_401()
    {
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        var partes = cidadao.Split('.');
        var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(partes[1])).Replace("\"Cidadao\"", "\"Admin\"");
        var adulterado = $"{partes[0]}.{Base64Url(payload)}.{partes[2]}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", adulterado)).StatusCode);
    }

    [Fact]
    public async Task TokenValidoGeradoPelaPropriaApi_Funciona()
    {
        var servico = new JwtTokenService(Options.Create(new JwtOptions { Secret = ApiFactory.SegredoJwt }), TimeProvider.System);
        var token = servico.Gerar(Fabrica.Admin(1)).Token;

        Assert.Equal(HttpStatusCode.OK, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", token)).StatusCode);
    }

    [Fact]
    public async Task LoginComEmailInexistenteEComSenhaErrada_MesmaResposta()
    {
        var (_, _) = await _http.CriarCidadaoAsync();
        var existente = HttpClientExtensions.NovoEmail();
        await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(existente), ApiFactory.Json);

        var semConta = await _http.PostAsJsonAsync("/auth/login", new { email = "nao-existe@teste.local", senha = "Errada123" });
        var senhaErrada = await _http.PostAsJsonAsync("/auth/login", new { email = existente, senha = "Errada123" });

        Assert.Equal(HttpStatusCode.Unauthorized, semConta.StatusCode);
        Assert.Equal(semConta.StatusCode, senhaErrada.StatusCode);
        Assert.Equal(await semConta.Content.ReadAsStringAsync(), await senhaErrada.Content.ReadAsStringAsync()); // sem enumeração de usuários
    }

    [Fact]
    public async Task ForcaBruta_ContaBloqueadaAposCincoSenhasErradas()
    {
        var email = HttpClientExtensions.NovoEmail();
        await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(email), ApiFactory.Json);

        for (var i = 1; i < Usuario.MaximoTentativasLogin; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await _http.PostAsJsonAsync("/auth/login", new { email, senha = "Errada123" })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await _http.PostAsJsonAsync("/auth/login", new { email, senha = "Errada123" })).StatusCode);

        // Nem a senha certa entra enquanto a conta estiver bloqueada.
        Assert.Equal(HttpStatusCode.TooManyRequests,
            (await _http.PostAsJsonAsync("/auth/login", new { email, senha = ApiFactory.SenhaPadrao })).StatusCode);
    }

    // ---------- Autorização (API1: BOLA / API5: BFLA) ----------

    [Fact]
    public async Task CidadaoNaoAcessaDadosDeOutroCidadao()
    {
        var (idAna, _) = await _http.CriarCidadaoAsync();
        var (_, bruno) = await _http.CriarCidadaoAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{idAna}", bruno)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Put, $"/cidadaos/{idAna}", bruno, new { nome = "Hack" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Delete, $"/cidadaos/{idAna}", bruno)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{idAna}/inscricoes", bruno)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{idAna}/recomendacoes", bruno)).StatusCode);
    }

    [Fact]
    public async Task PerfilErrado_403EmFuncoesDeOutroPerfil()
    {
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        var (_, org) = await _http.CriarOrganizacaoAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Post, "/oportunidades", cidadao, HttpClientExtensions.NovaOportunidade())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", org)).StatusCode);
        Assert.Equal("Acesso negado!", (await (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", org)).JsonAsync()).GetProperty("mensagem").GetString());
    }

    // ---------- Entrada maliciosa (API3/API8: injeção, mass assignment) ----------

    [Fact]
    public async Task MassAssignment_CamposExtrasSaoIgnorados()
    {
        var email = HttpClientExtensions.NovoEmail();
        var resposta = await _http.PostAsJsonAsync("/cidadaos", new
        {
            id = 1, perfil = "Admin", ativo = false, senhaHash = "x", tentativasLoginFalhas = 99,
            nome = "Tentativa", email, senha = ApiFactory.SenhaPadrao, dataNascimento = "2000-01-01",
            escolaridade = "MedioCompleto", areasInteresse = new[] { "Artes" }, endereco = HttpClientExtensions.Endereco("Centro")
        }, ApiFactory.Json);

        var corpo = await resposta.JsonAsync();
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotEqual(1, corpo.GetProperty("id").GetInt64());
        Assert.True(corpo.GetProperty("ativo").GetBoolean());
        var token = await _http.LoginAsync(email);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", token)).StatusCode); // não virou admin
    }

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("'; DROP TABLE oportunidades; --")]
    [InlineData("%' UNION SELECT senha_hash FROM usuarios --")]
    public async Task SqlInjectionNaBusca_NaoAfetaOBanco(string ataque)
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        await _http.CriarOportunidadeAsync(org);

        var resposta = await _http.GetAsync($"/oportunidades?texto={Uri.EscapeDataString(ataque)}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(0, (await resposta.JsonAsync()).GetProperty("totalItens").GetInt32());
        Assert.True((await (await _http.GetAsync("/oportunidades")).JsonAsync()).GetProperty("totalItens").GetInt32() > 0);
    }

    [Fact]
    public async Task XssArmazenado_DevolvidoComoJsonInerte()
    {
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        const string payload = "<script>alert('xss')</script>";

        var criada = await _http.EnviarAsync(HttpMethod.Post, "/propostas", cidadao,
            new { titulo = payload, descricao = "D", area = "Artes", bairro = "Centro" });
        var lida = await _http.GetAsync($"/propostas/{(await criada.JsonAsync()).GetProperty("id").GetInt64()}");

        Assert.Equal("application/json", lida.Content.Headers.ContentType!.MediaType);
        Assert.Equal("nosniff", lida.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.DoesNotContain("<script>", await lida.Content.ReadAsStringAsync()); // o JSON escapa < e >
        Assert.Equal(payload, (await lida.JsonAsync()).GetProperty("titulo").GetString());
    }

    [Fact]
    public async Task JsonMalformado_400SemVazarDetalhesInternos()
    {
        var resposta = await _http.PostAsync("/cidadaos", new StringContent("{ isso não é json", Encoding.UTF8, "application/json"));
        var texto = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.DoesNotContain("Oportuniza.", texto);
        Assert.DoesNotContain("Exception", texto);
    }

    [Fact]
    public async Task EnumComoNumero_Recusado()
    {
        var corpo = new
        {
            nome = "N", email = HttpClientExtensions.NovoEmail(), senha = ApiFactory.SenhaPadrao, dataNascimento = "2000-01-01",
            escolaridade = 999, areasInteresse = new[] { "Artes" }, endereco = HttpClientExtensions.Endereco("Centro")
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.PostAsJsonAsync("/cidadaos", corpo)).StatusCode);
    }

    [Fact]
    public async Task CamposGigantes_Recusados()
    {
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        var resposta = await _http.EnviarAsync(HttpMethod.Post, "/propostas", cidadao,
            new { titulo = new string('a', 5000), descricao = "D", area = "Artes", bairro = "Centro" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Paginacao_TamanhoAbusivoELimitadoA50()
    {
        var pagina = await (await _http.GetAsync("/oportunidades?tamanho=100000&pagina=-3")).JsonAsync();

        Assert.Equal(50, pagina.GetProperty("tamanhoPagina").GetInt32());
        Assert.Equal(1, pagina.GetProperty("pagina").GetInt32());
    }

    // ---------- Configuração segura (API8: Security Misconfiguration) ----------

    [Theory]
    [InlineData("/health")]
    [InlineData("/oportunidades")]
    public async Task CabecalhosDeSeguranca_EmTodasAsRespostas(string rota)
    {
        var resposta = await _http.GetAsync(rota);

        Assert.Equal("nosniff", resposta.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", resposta.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", resposta.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("frame-ancestors 'none'", resposta.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Contains("no-store", resposta.Headers.CacheControl!.ToString());
        Assert.False(resposta.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Swagger_TemCspPropriaSemNoStore()
    {
        var resposta = await _http.GetAsync("/swagger/index.html");

        Assert.Contains("script-src 'self'", resposta.Headers.GetValues("Content-Security-Policy").Single());
        Assert.NotEqual(true, resposta.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task Cors_SoLiberaOrigensConfiguradas()
    {
        async Task<HttpResponseMessage> Preflight(string origem)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/oportunidades");
            request.Headers.Add("Origin", origem);
            request.Headers.Add("Access-Control-Request-Method", "GET");
            return await _http.SendAsync(request);
        }

        var permitida = await Preflight("http://localhost:5173");
        var atacante = await Preflight("https://site-malicioso.com");

        Assert.Equal("http://localhost:5173", permitida.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(atacante.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task RotaInexistenteEMetodoErrado_SemDetalhes()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/rota-que-nao-existe")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _http.DeleteAsync("/oportunidades")).StatusCode);
    }

    private static string Base64Url(string texto) => Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(texto));
}

/// <summary>Rate limiting do login por IP.</summary>
public class RateLimitTests(ApiFactoryComLimiteDeLogin factory) : IClassFixture<ApiFactoryComLimiteDeLogin>
{
    [Fact]
    public async Task Login_AcimaDoLimitePorMinuto_429()
    {
        var http = factory.CreateClient();
        var tentativa = () => http.PostAsJsonAsync("/auth/login", new { email = "x@teste.local", senha = "Errada123" });

        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await tentativa()).StatusCode);
        var bloqueada = await tentativa();

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
        Assert.Contains("Muitas requisições", (await bloqueada.JsonAsync()).GetProperty("mensagem").GetString());
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync("/health")).StatusCode); // o resto da API segue normal
    }
}

/// <summary>Configuração de produção: HTTPS obrigatório e Swagger desligado.</summary>
public class ProducaoTests(ApiFactoryProducao factory) : IClassFixture<ApiFactoryProducao>
{
    [Fact]
    public async Task Http_RedirecionaParaHttps()
    {
        var resposta = await factory.CreateClient(new() { AllowAutoRedirect = false }).GetAsync("/oportunidades");

        Assert.Equal(HttpStatusCode.TemporaryRedirect, resposta.StatusCode);
        Assert.Equal("https", resposta.Headers.Location!.Scheme);
    }

    [Fact]
    public async Task Https_EnviaHsts_ESwaggerFicaDesligado()
    {
        // O ASP.NET não envia HSTS para "localhost", então simulamos um domínio real.
        var http = factory.CreateClient(new() { BaseAddress = new Uri("https://api.oportuniza.teste") });

        var resposta = await http.GetAsync("/oportunidades");
        Assert.Contains("max-age=31536000", resposta.Headers.GetValues("Strict-Transport-Security").Single());
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }
}

/// <summary>Subida com migrações (como em desenvolvimento): aplica as 6 migrações e o seed fictício.</summary>
public class MigracoesNaSubidaTests(ApiFactoryComMigracoes factory) : IClassFixture<ApiFactoryComMigracoes>
{
    [Fact]
    public async Task SeedPermiteLoginDoAdminDeDemonstracao()
    {
        var http = factory.CreateClient();

        var resposta = await http.PostAsJsonAsync("/auth/login", new { email = "admin@oportuniza.local", senha = ApiFactory.SenhaPadrao });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var oportunidades = await (await http.GetAsync("/oportunidades")).JsonAsync();
        Assert.Equal(7, oportunidades.GetProperty("totalItens").GetInt32());
    }
}
