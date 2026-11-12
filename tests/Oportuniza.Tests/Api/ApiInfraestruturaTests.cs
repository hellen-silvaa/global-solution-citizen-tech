using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Tests.Api;

public class GlobalExceptionHandlerTests
{
    public static TheoryData<Exception, int> Excecoes => new()
    {
        { new ValidacaoException("regra"), 400 },
        { new BadHttpRequestException("corpo"), 400 },
        { new CredenciaisInvalidasException(), 401 },
        { new AcessoNegadoException(), 403 },
        { new RecursoNaoEncontradoException("sumiu"), 404 },
        { new DbUpdateConcurrencyException("concorrência"), 409 },
        { new DbUpdateException("duplicado"), 409 },
        { new MuitasTentativasException(), 429 },
        { new IntegracaoExternaException("viacep"), 503 },
        { new InvalidOperationException("detalhe interno com SQL e stack"), 500 }
    };

    [Theory]
    [MemberData(nameof(Excecoes))]
    public async Task TraduzCadaExcecaoParaOStatusCorreto(Exception excecao, int statusEsperado)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        var tratado = await new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance)
            .TryHandleAsync(context, excecao, CancellationToken.None);

        context.Response.Body.Position = 0;
        var corpo = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        Assert.True(tratado);
        Assert.Equal(statusEsperado, context.Response.StatusCode);
        Assert.True(corpo.TryGetProperty("mensagem", out var mensagem));
        if (statusEsperado == 500)
            Assert.Equal("Erro interno no servidor!", mensagem.GetString()); // nunca vaza detalhes internos
    }
}

public class RateLimitingConfigurationTests
{
    [Fact]
    public void ChaveDoCliente_UsaOIpOuUmBaldeComum()
    {
        var comIp = new DefaultHttpContext();
        comIp.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("200.10.20.30");

        Assert.Equal("200.10.20.30", RateLimitingConfiguration.ChaveDoCliente(comIp));
        Assert.Equal("desconhecido", RateLimitingConfiguration.ChaveDoCliente(new DefaultHttpContext()));
    }
}

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "teste"));

    [Fact]
    public void ConverteIdEPerfil()
    {
        var usuario = Principal(new Claim(ClaimTypes.NameIdentifier, "10"), new Claim(ClaimTypes.Role, "Cidadao")).ParaUsuarioAutenticado();

        Assert.Equal(10, usuario.Id);
        Assert.Equal(Perfil.Cidadao, usuario.Perfil);
    }

    [Fact]
    public void AceitaClaimSubQuandoNaoHaNameIdentifier() =>
        Assert.Equal(7, Principal(new Claim("sub", "7"), new Claim(ClaimTypes.Role, "Admin")).ParaUsuarioAutenticado().Id);

    [Theory]
    [InlineData(null, "Cidadao")]
    [InlineData("abc", "Cidadao")]
    [InlineData("10", null)]
    [InlineData("10", "SuperUsuario")]
    public void ClaimsAusentesOuInvalidas_CredenciaisInvalidas(string? id, string? perfil)
    {
        var claims = new List<Claim>();
        if (id is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        if (perfil is not null) claims.Add(new Claim(ClaimTypes.Role, perfil));

        Assert.Throws<CredenciaisInvalidasException>(() => Principal([.. claims]).ParaUsuarioAutenticado());
    }
}

public class ValidacaoResponseTests
{
    private static List<DadosErroValidacao> Erros(Action<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary> preencher)
    {
        var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        preencher(context.ModelState);
        var resultado = Assert.IsType<BadRequestObjectResult>(ValidacaoResponse.Criar(context));
        return Assert.IsType<List<DadosErroValidacao>>(resultado.Value);
    }

    [Fact]
    public void MensagemDeValidacao_EMantida() =>
        Assert.Equal(new DadosErroValidacao("Email", "E-mail inválido"), Erros(m => m.AddModelError("Email", "E-mail inválido")).Single());

    [Fact]
    public void MensagemTecnicaDoJson_EOcultada()
    {
        var erro = Erros(m => m.AddModelError("$.dataNascimento",
            "The JSON value could not be converted to Oportuniza.Application.Dtos.Usuarios.DadosCadastroCidadao")).Single();

        Assert.Equal("dataNascimento", erro.Campo);
        Assert.DoesNotContain("Oportuniza", erro.Mensagem);
    }

    [Fact]
    public void ExcecaoInterna_EOcultada() =>
        Assert.Equal("Valor inválido ou em formato incorreto",
            Erros(m => m.AddModelError("idade", new FormatException("detalhe"), new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider().GetMetadataForType(typeof(int)))).Single().Mensagem);

    [Theory]
    [InlineData("dados", "The dados field is required.", "dados", "Campo obrigatório")]
    [InlineData("$", "", "corpo", "Valor inválido ou em formato incorreto")]
    [InlineData("", "A non-empty request body is required.", "corpo", "Corpo da requisição é obrigatório")]
    [InlineData("idade", "The value 'abc' is not valid.", "idade", "Valor inválido ou em formato incorreto")]
    public void MensagensPadraoEmIngles_SaoTraduzidas(string chave, string original, string campo, string esperado) =>
        Assert.Equal(new DadosErroValidacao(campo, esperado), Erros(m => m.AddModelError(chave, original)).Single());
}
