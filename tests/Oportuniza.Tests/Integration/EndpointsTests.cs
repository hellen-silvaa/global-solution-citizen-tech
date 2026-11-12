using System.Net;
using System.Net.Http.Json;
using Oportuniza.Application.Inscricoes.Regras;

namespace Oportuniza.Tests.Integration;

/// <summary>Cada endpoint pelo HTTP: caminho feliz, erros de regra, permissões e recursos inexistentes.</summary>
public class EndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _http = factory.CreateClient();

    // ---------- Cidadãos ----------

    [Fact]
    public async Task Cidadao_CadastroDetalheAtualizacaoEDesativacao()
    {
        var email = HttpClientExtensions.NovoEmail();
        var cadastro = await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(email), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);
        Assert.NotNull(cadastro.Headers.Location);
        var id = (await cadastro.JsonAsync()).GetProperty("id").GetInt64();
        var token = await _http.LoginAsync(email);

        var detalhe = await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{id}", token);
        Assert.Equal(email, (await detalhe.JsonAsync()).GetProperty("email").GetString());

        var atualizado = await _http.EnviarAsync(HttpMethod.Put, $"/cidadaos/{id}", token,
            new { nome = "Nome Novo", escolaridade = "Tecnico", areasInteresse = new[] { "Idiomas" } });
        Assert.Equal("Nome Novo", (await atualizado.JsonAsync()).GetProperty("nome").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Delete, $"/cidadaos/{id}", token)).StatusCode);
        var loginDepois = await _http.PostAsJsonAsync("/auth/login", new { email, senha = ApiFactory.SenhaPadrao });
        Assert.Equal(HttpStatusCode.Unauthorized, loginDepois.StatusCode);
    }

    [Fact]
    public async Task Cidadao_ListagemSoParaAdmin()
    {
        var (_, cidadao) = await _http.CriarCidadaoAsync("Bairro Listagem");
        var admin = await factory.CriarAdminETokenAsync(_http);

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos", cidadao)).StatusCode);
        var lista = await (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos?bairro=Bairro Listagem", admin)).JsonAsync();
        Assert.Equal(1, lista.GetProperty("totalItens").GetInt32());
        Assert.False(lista.GetProperty("itens")[0].TryGetProperty("email", out _)); // minimização de dados
    }

    [Fact]
    public async Task Cidadao_InexistenteParaAdmin_404()
    {
        var admin = await factory.CriarAdminETokenAsync(_http);
        Assert.Equal(HttpStatusCode.NotFound, (await _http.EnviarAsync(HttpMethod.Get, "/cidadaos/999999", admin)).StatusCode);
    }

    [Theory]
    [InlineData("curta1")]
    [InlineData("somenteletras")]
    [InlineData("12345678")]
    public async Task Cidadao_SenhaFraca_400(string senha)
    {
        var resposta = await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(HttpClientExtensions.NovoEmail(), senha: senha), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains((await resposta.JsonAsync()).EnumerateArray(), e => e.GetProperty("campo").GetString() == "Senha");
    }

    [Fact]
    public async Task Cidadao_EmailRepetido_400()
    {
        var email = HttpClientExtensions.NovoEmail();
        await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(email), ApiFactory.Json);

        var repetido = await _http.PostAsJsonAsync("/cidadaos", HttpClientExtensions.NovoCidadao(email.ToUpperInvariant()), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, repetido.StatusCode);
        Assert.Equal("E-mail já cadastrado!", (await repetido.JsonAsync()).GetProperty("mensagem").GetString());
    }

    // ---------- Organizações ----------

    [Fact]
    public async Task Organizacao_PublicaParaConsulta_EditavelSoPelaPropria()
    {
        var (id, token) = await _http.CriarOrganizacaoAsync();
        var (_, outra) = await _http.CriarOrganizacaoAsync();

        Assert.Equal(HttpStatusCode.OK, (await _http.GetAsync($"/organizacoes/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _http.GetAsync("/organizacoes?tipo=Ong&tamanho=5")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/organizacoes/999999")).StatusCode);

        var atualizacao = new { descricao = "Nova descrição", site = "https://novo.org" };
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Put, $"/organizacoes/{id}", outra, atualizacao)).StatusCode);
        var atualizada = await (await _http.EnviarAsync(HttpMethod.Put, $"/organizacoes/{id}", token, atualizacao)).JsonAsync();
        Assert.Equal("https://novo.org", atualizada.GetProperty("site").GetString());

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Delete, $"/organizacoes/{id}", outra)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Delete, $"/organizacoes/{id}", token)).StatusCode);
    }

    [Fact]
    public async Task Organizacao_SiteJavascript_400()
    {
        var resposta = await _http.PostAsJsonAsync("/organizacoes", new
        {
            nome = "ONG", email = HttpClientExtensions.NovoEmail(), senha = ApiFactory.SenhaPadrao, tipo = "Ong",
            descricao = "D", site = "javascript:alert(1)", endereco = HttpClientExtensions.Endereco("Centro")
        }, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    // ---------- Oportunidades ----------

    [Fact]
    public async Task Oportunidade_CicloDaOrganizacao()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var (_, outra) = await _http.CriarOrganizacaoAsync();
        var id = await _http.CriarOportunidadeAsync(org);

        Assert.Equal(HttpStatusCode.OK, (await _http.GetAsync($"/oportunidades/{id}")).StatusCode);

        var atualizacao = new
        {
            titulo = "Título novo", descricao = "D", modalidade = "Presencial", area = "Artes", bairro = "Centro",
            cargaHoraria = 10, vagas = 3, dataInicio = DateTime.UtcNow.AddDays(5), dataFim = DateTime.UtcNow.AddDays(6)
        };
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Put, $"/oportunidades/{id}", outra, atualizacao)).StatusCode);
        var atualizada = await (await _http.EnviarAsync(HttpMethod.Put, $"/oportunidades/{id}", org, atualizacao)).JsonAsync();
        Assert.Equal("Título novo", atualizada.GetProperty("titulo").GetString());

        Assert.Equal(HttpStatusCode.OK, (await _http.EnviarAsync(HttpMethod.Get, $"/oportunidades/{id}/inscricoes", org)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Patch, $"/oportunidades/{id}/encerrar", org)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.EnviarAsync(HttpMethod.Patch, $"/oportunidades/{id}/encerrar", org)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Patch, $"/oportunidades/{id}/cancelar", org)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/oportunidades/999999")).StatusCode);
    }

    [Fact]
    public async Task Oportunidade_DataNoPassado_400()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var resposta = await _http.EnviarAsync(HttpMethod.Post, "/oportunidades", org, HttpClientExtensions.NovaOportunidade(diasParaInicio: -1));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Oportunidade_BuscaFiltraEPagina()
    {
        var (orgId, org) = await _http.CriarOrganizacaoAsync();
        for (var i = 0; i < 3; i++)
            await _http.CriarOportunidadeAsync(org, area: "Beleza");

        var pagina = await (await _http.GetAsync($"/oportunidades?area=Beleza&organizacaoId={orgId}&somenteComVagas=true&tamanho=2")).JsonAsync();

        Assert.Equal(2, pagina.GetProperty("itens").GetArrayLength());
        Assert.Equal(3, pagina.GetProperty("totalItens").GetInt32());
        Assert.Equal(2, pagina.GetProperty("totalPaginas").GetInt32());
    }

    // ---------- Inscrições ----------

    [Fact]
    public async Task Inscricao_FluxoCompletoAteOIndicador()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var (cidadaoId, cidadao) = await _http.CriarCidadaoAsync();
        var oportunidade = await _http.CriarOportunidadeAsync(org);

        var recomendacoes = await (await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{cidadaoId}/recomendacoes", cidadao)).JsonAsync();
        Assert.Contains(recomendacoes.EnumerateArray(), r => r.GetProperty("oportunidade").GetProperty("id").GetInt64() == oportunidade);

        var inscricao = await _http.InscreverAsync(oportunidade, cidadao);
        Assert.Equal(HttpStatusCode.OK, (await _http.EnviarAsync(HttpMethod.Get, $"/inscricoes/{inscricao}", cidadao)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/confirmar", org)).StatusCode);
        var concluida = await (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/concluir", org)).JsonAsync();
        Assert.Equal("Concluida", concluida.GetProperty("status").GetString());

        var minhas = await (await _http.EnviarAsync(HttpMethod.Get, $"/cidadaos/{cidadaoId}/inscricoes?status=Concluida", cidadao)).JsonAsync();
        Assert.Equal(1, minhas.GetArrayLength());

        var impacto = await (await _http.GetAsync("/indicadores/impacto")).JsonAsync();
        Assert.True(impacto.GetProperty("inscricoesConcluidas").GetInt32() >= 1);
    }

    [Fact]
    public async Task Inscricao_RegrasDeNegocio()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var (_, ana) = await _http.CriarCidadaoAsync();
        var (_, bruno) = await _http.CriarCidadaoAsync();
        var umaVaga = await _http.CriarOportunidadeAsync(org, vagas: 1);
        var url = $"/oportunidades/{umaVaga}/inscricoes";

        await _http.InscreverAsync(umaVaga, ana);

        var duplicada = await _http.EnviarAsync(HttpMethod.Post, url, ana);
        Assert.Contains("inscrição ativa", (await duplicada.JsonAsync()).GetProperty("mensagem").GetString()); // RN-I3

        var semVaga = await _http.EnviarAsync(HttpMethod.Post, url, bruno);
        Assert.Contains("vagas", (await semVaga.JsonAsync()).GetProperty("mensagem").GetString()); // RN-I2

        Assert.Equal(HttpStatusCode.NotFound, (await _http.EnviarAsync(HttpMethod.Post, "/oportunidades/999999/inscricoes", ana)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Post, url, org)).StatusCode);
    }

    [Fact]
    public async Task Inscricao_LimiteDeInscricoesAtivas_RN_I7()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        for (var i = 0; i < RegraLimiteInscricoesAtivas.Limite; i++)
            await _http.InscreverAsync(await _http.CriarOportunidadeAsync(org), cidadao);

        var excedente = await _http.EnviarAsync(HttpMethod.Post, $"/oportunidades/{await _http.CriarOportunidadeAsync(org)}/inscricoes", cidadao);

        Assert.Equal(HttpStatusCode.BadRequest, excedente.StatusCode);
        Assert.Contains("Limite", (await excedente.JsonAsync()).GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task Inscricao_PermissoesECancelamento()
    {
        var (_, org) = await _http.CriarOrganizacaoAsync();
        var (_, cidadao) = await _http.CriarCidadaoAsync();
        var (_, intruso) = await _http.CriarCidadaoAsync();
        var oportunidade = await _http.CriarOportunidadeAsync(org, vagas: 1);
        var inscricao = await _http.InscreverAsync(oportunidade, cidadao);

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Get, $"/inscricoes/{inscricao}", intruso)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/confirmar", cidadao)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/concluir", org)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/cancelar", intruso)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/cancelar", cidadao)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.EnviarAsync(HttpMethod.Patch, $"/inscricoes/{inscricao}/cancelar", cidadao)).StatusCode);

        var vagaDeVolta = await (await _http.GetAsync($"/oportunidades/{oportunidade}")).JsonAsync();
        Assert.Equal(1, vagaDeVolta.GetProperty("vagasDisponiveis").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await _http.EnviarAsync(HttpMethod.Get, "/inscricoes/999999", cidadao)).StatusCode);
    }

    // ---------- Propostas ----------

    [Fact]
    public async Task Proposta_ParticipacaoCidadaCompleta()
    {
        var (_, autora) = await _http.CriarCidadaoAsync("Jardim Ângela");
        var (_, vizinho) = await _http.CriarCidadaoAsync("Jardim Ângela");
        var (_, org) = await _http.CriarOrganizacaoAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Post, "/propostas", org,
            new { titulo = "x", descricao = "x", area = "Saude", bairro = "x" })).StatusCode);

        var criada = await _http.EnviarAsync(HttpMethod.Post, "/propostas", autora,
            new { titulo = "Cuidador de idosos", descricao = "Falta no bairro", area = "Saude", bairro = "Jardim Ângela" });
        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        var id = (await criada.JsonAsync()).GetProperty("id").GetInt64();

        Assert.Equal(HttpStatusCode.OK, (await _http.EnviarAsync(HttpMethod.Post, $"/propostas/{id}/votos", vizinho)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.EnviarAsync(HttpMethod.Post, $"/propostas/{id}/votos", vizinho)).StatusCode);

        var lista = await (await _http.GetAsync("/propostas?bairro=Jardim Ângela&status=Aberta")).JsonAsync();
        Assert.Equal(1, lista.GetProperty("itens")[0].GetProperty("totalVotos").GetInt32());

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Post, $"/propostas/{id}/atender", autora,
            HttpClientExtensions.NovaOportunidade(area: "Saude"))).StatusCode);
        var atendida = await _http.EnviarAsync(HttpMethod.Post, $"/propostas/{id}/atender", org, HttpClientExtensions.NovaOportunidade(area: "Saude"));
        Assert.Equal(HttpStatusCode.Created, atendida.StatusCode);

        var depois = await (await _http.GetAsync($"/propostas/{id}")).JsonAsync();
        Assert.Equal("Atendida", depois.GetProperty("status").GetString());
        Assert.Equal((await atendida.JsonAsync()).GetProperty("id").GetInt64(), depois.GetProperty("oportunidadeGeradaId").GetInt64());
        Assert.Equal(HttpStatusCode.BadRequest, (await _http.EnviarAsync(HttpMethod.Post, $"/propostas/{id}/atender", org,
            HttpClientExtensions.NovaOportunidade())).StatusCode);
    }

    [Fact]
    public async Task Proposta_ArquivarSoAutor()
    {
        var (_, autora) = await _http.CriarCidadaoAsync();
        var (_, outro) = await _http.CriarCidadaoAsync();
        var id = (await (await _http.EnviarAsync(HttpMethod.Post, "/propostas", autora,
            new { titulo = "T", descricao = "D", area = "Artes", bairro = "Centro" })).JsonAsync()).GetProperty("id").GetInt64();

        Assert.Equal(HttpStatusCode.Forbidden, (await _http.EnviarAsync(HttpMethod.Patch, $"/propostas/{id}/arquivar", outro)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _http.EnviarAsync(HttpMethod.Patch, $"/propostas/{id}/arquivar", autora)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/propostas/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _http.GetAsync("/propostas?ordenarPorVotos=false")).StatusCode);
    }

    // ---------- Consultas públicas ----------

    [Theory]
    [InlineData("01001-000", HttpStatusCode.OK)]
    [InlineData("99999999", HttpStatusCode.NotFound)]
    [InlineData("00000000", HttpStatusCode.ServiceUnavailable)]
    [InlineData("123", HttpStatusCode.BadRequest)]
    public async Task ViaCep_MapeiaRespostasDoServicoExterno(string cep, HttpStatusCode esperado)
    {
        var resposta = await _http.GetAsync($"/enderecos/{cep}");

        Assert.Equal(esperado, resposta.StatusCode);
        if (esperado == HttpStatusCode.OK)
            Assert.Equal("Sé", (await resposta.JsonAsync()).GetProperty("bairro").GetString());
    }

    [Fact]
    public async Task Health_RespondeUpComBanco()
    {
        var health = await (await _http.GetAsync("/health")).JsonAsync();

        Assert.Equal("UP", health.GetProperty("status").GetString());
        Assert.Equal("UP", health.GetProperty("banco").GetString());
    }

    [Fact]
    public async Task Swagger_DocumentaEndpointsEEsquemaBearer_ERaizRedireciona()
    {
        var doc = await (await _http.GetAsync("/swagger/v1/swagger.json")).JsonAsync();

        var rotas = doc.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains("/oportunidades/{id}/inscricoes", rotas);
        Assert.Contains("/indicadores/impacto", rotas);
        Assert.True(doc.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));

        var raiz = await factory.CreateClient(new() { AllowAutoRedirect = false }).GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, raiz.StatusCode);
    }
}
