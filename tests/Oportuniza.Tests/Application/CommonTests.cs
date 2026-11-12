using Moq;
using Oportuniza.Application.Common;
using Oportuniza.Application.Common.Validation;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application;

public class PaginacaoTests
{
    [Theory]
    [InlineData(1, 10, 1, 10, 0)]
    [InlineData(3, 10, 3, 10, 20)]
    [InlineData(0, 10, 1, 10, 0)]
    [InlineData(-5, 0, 1, 1, 0)]
    [InlineData(2, 999, 2, 50, 50)]
    public void Normaliza_PaginaETamanho(int pagina, int tamanho, int paginaEsperada, int tamanhoEsperado, int ignorarEsperado)
    {
        var paginacao = new Paginacao(pagina, tamanho);

        Assert.Equal(paginaEsperada, paginacao.PaginaNormalizada);
        Assert.Equal(tamanhoEsperado, paginacao.TamanhoNormalizado);
        Assert.Equal(ignorarEsperado, paginacao.Ignorar);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(5, 0, 0)]
    public void TotalPaginas_ArredondaParaCima(int total, int tamanho, int esperado) =>
        Assert.Equal(esperado, new PaginaResultado<int>([], 1, tamanho, total).TotalPaginas);

    [Fact]
    public void Mapear_ConverteItensEMantemPaginacao()
    {
        var pagina = new PaginaResultado<int>([1, 2], 2, 2, 5).Mapear(i => i * 10);

        Assert.Equal([10, 20], pagina.Itens);
        Assert.Equal(2, pagina.Pagina);
        Assert.Equal(5, pagina.TotalItens);
    }
}

public class UsuarioAutenticadoTests
{
    [Fact]
    public void ExigirPerfil_PerfilDiferente_AcessoNegado()
    {
        var cidadao = new UsuarioAutenticado(1, Perfil.Cidadao);

        cidadao.ExigirPerfil(Perfil.Cidadao);
        Assert.Throws<AcessoNegadoException>(() => cidadao.ExigirPerfil(Perfil.Organizacao));
    }

    [Fact]
    public void ExigirProprioOuAdmin_SoODonoOuAdminPassam()
    {
        new UsuarioAutenticado(1, Perfil.Cidadao).ExigirProprioOuAdmin(1);
        new UsuarioAutenticado(99, Perfil.Admin).ExigirProprioOuAdmin(1);

        Assert.Throws<AcessoNegadoException>(() => new UsuarioAutenticado(2, Perfil.Cidadao).ExigirProprioOuAdmin(1));
        Assert.True(new UsuarioAutenticado(99, Perfil.Admin).EhAdmin);
    }
}

public class RepositorioExtensionsTests
{
    [Fact]
    public async Task ObterObrigatorio_Existente_DevolveEInexistente_Lanca404()
    {
        var oportunidade = Fabrica.Oportunidade(30);
        var repositorio = new Mock<IOportunidadeRepository>();
        repositorio.Setup(r => r.ObterPorIdAsync(30, It.IsAny<CancellationToken>())).ReturnsAsync(oportunidade);

        Assert.Same(oportunidade, await repositorio.Object.ObterObrigatorioAsync(30, "x"));
        var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => repositorio.Object.ObterObrigatorioAsync<Oportunidade>(31, "Não achei!"));
        Assert.Equal("Não achei!", erro.Message);
    }
}

public class SenhaForteTests
{
    [Theory]
    [InlineData("Senha@123", true)]
    [InlineData("abcdefg1", true)]
    [InlineData("curta1", false)]           // menos de 8
    [InlineData("somenteletras", false)]    // sem número
    [InlineData("12345678", false)]         // sem letra
    [InlineData(" Senha123", false)]        // espaço no início
    [InlineData("Senha123 ", false)]        // espaço no fim
    [InlineData(null, false)]
    public void Valida_PoliticaDeSenha(string? senha, bool valida) =>
        Assert.Equal(valida, new SenhaForteAttribute().IsValid(senha));

    [Fact]
    public void SenhaMuitoLonga_EInvalida() =>
        Assert.False(new SenhaForteAttribute().IsValid("a1" + new string('x', 63)));

    [Fact]
    public void NaoString_EInvalida() => Assert.False(new SenhaForteAttribute().IsValid(12345678));
}

public class DtoMapeamentoTests
{
    [Fact]
    public void DadosEndereco_IdaEVolta()
    {
        var dto = new DadosEndereco("05863-000", "Rua", "1", "Centro", "São Paulo", "SP");

        var vo = dto.ParaValueObject();

        Assert.Equal("05863000", vo.Cep);
        Assert.Equal(dto with { Cep = "05863000" }, DadosEndereco.De(vo));
        Assert.Null(DadosEndereco.De(null));
    }

    [Fact]
    public void Detalhamentos_ExpoemValorDoEmailENaoASenha()
    {
        var cidadao = Fabrica.Cidadao(10);
        var organizacao = Fabrica.Organizacao(20);

        var dtoCidadao = DadosDetalhamentoCidadao.De(cidadao);
        var dtoOrganizacao = DadosDetalhamentoOrganizacao.De(organizacao);

        Assert.Equal("cidadao10@teste.local", dtoCidadao.Email);
        Assert.Equal("org20@teste.local", dtoOrganizacao.Email);
        Assert.DoesNotContain(typeof(DadosDetalhamentoCidadao).GetProperties(), p => p.Name.Contains("Senha"));
        Assert.Equal("Capão Redondo", DadosListagemCidadao.De(cidadao).Bairro);
    }

    [Fact]
    public void DetalhamentoInscricaoEProposta_UsamOsAgregados()
    {
        var inscricao = Fabrica.Inscricao(40);
        var proposta = Fabrica.Proposta(50);
        proposta.Atender(Fabrica.Oportunidade(31));

        var dtoInscricao = DadosDetalhamentoInscricao.De(inscricao);
        var dtoProposta = DadosDetalhamentoProposta.De(proposta);

        Assert.Equal(StatusInscricao.Pendente, dtoInscricao.Status);
        Assert.Equal("Curso de teste", dtoInscricao.OportunidadeTitulo);
        Assert.Equal(31, dtoProposta.OportunidadeGeradaId);
        Assert.Equal(StatusProposta.Atendida, dtoProposta.Status);
    }
}
