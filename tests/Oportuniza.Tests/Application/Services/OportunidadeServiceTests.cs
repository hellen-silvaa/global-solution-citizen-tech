using Moq;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Recomendacoes;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

public class OportunidadeServiceTests : CenarioDeServico
{
    private OportunidadeService Servico() => new(Oportunidades.Object, Organizacoes.Object, Cidadaos.Object,
        Inscricoes.Object, new MotorDeRecomendacao([new CriterioInteresse(), new CriterioMesmoBairro()]),
        UnitOfWork.Object, Relogio);

    private static DadosCriacaoOportunidade Criacao(int diasParaInicio = 10) => new("Curso", "Descrição",
        TipoOportunidade.Curso, Modalidade.Online, AreaConhecimento.Tecnologia, "Centro", 10, 5,
        Fabrica.Agora.AddDays(diasParaInicio), Fabrica.Agora.AddDays(diasParaInicio + 5));

    private static DadosAtualizacaoOportunidade Atualizacao() => new("Novo", "Nova", Modalidade.Hibrida,
        AreaConhecimento.Artes, "Centro", 20, 8, Fabrica.Agora.AddDays(3), Fabrica.Agora.AddDays(9));

    [Fact]
    public async Task Criar_OrganizacaoAtiva_Salva()
    {
        Existe(Fabrica.Organizacao(20));

        var resultado = await Servico().CriarAsync(Criacao(), ComoOrganizacao(20));

        Assert.Equal(20, resultado.OrganizacaoId);
        Assert.Equal(5, resultado.VagasDisponiveis);
        Oportunidades.Verify(r => r.AdicionarAsync(It.IsAny<Oportunidade>(), It.IsAny<CancellationToken>()), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Criar_PerfilCidadao_AcessoNegado() =>
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().CriarAsync(Criacao(), ComoCidadao()));

    [Fact]
    public async Task Criar_OrganizacaoInexistente_404() =>
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().CriarAsync(Criacao(), ComoOrganizacao(20)));

    [Fact]
    public async Task Criar_OrganizacaoInativa_LancaValidacao()
    {
        Existe(Fabrica.Organizacao(20)).Desativar();
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().CriarAsync(Criacao(), ComoOrganizacao(20)));
        NaoSalvou();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Criar_DataDeInicioNaoFutura_LancaValidacao(int diasParaInicio)
    {
        Existe(Fabrica.Organizacao(20));
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().CriarAsync(Criacao(diasParaInicio), ComoOrganizacao(20)));
    }

    [Fact]
    public async Task BuscarEObter()
    {
        var oportunidade = Existe(Fabrica.Oportunidade(30));
        var filtro = new FiltroOportunidades(Area: AreaConhecimento.Tecnologia);
        Oportunidades.Setup(r => r.BuscarAsync(filtro, It.IsAny<Paginacao>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginaResultado<Oportunidade>([oportunidade], 1, 10, 1));

        Assert.Single((await Servico().BuscarAsync(filtro, new Paginacao())).Itens);
        Assert.Equal(30, (await Servico().ObterAsync(30)).Id);
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().ObterAsync(31));
    }

    [Fact]
    public async Task Atualizar_PelaDonaOuAdmin()
    {
        var oportunidade = Existe(Fabrica.Oportunidade(30, Fabrica.Organizacao(20)));

        await Servico().AtualizarAsync(30, Atualizacao(), ComoOrganizacao(20));
        Assert.Equal("Novo", oportunidade.Titulo);

        await Servico().AtualizarAsync(30, Atualizacao() with { Titulo = "Pelo admin" }, Admin);
        Assert.Equal("Pelo admin", oportunidade.Titulo);
        Salvou(2);
    }

    [Fact]
    public async Task AlterarOportunidadeDeOutraOrganizacao_AcessoNegado_RN_O1()
    {
        Existe(Fabrica.Oportunidade(30, Fabrica.Organizacao(20)));
        var outra = ComoOrganizacao(21);

        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().AtualizarAsync(30, Atualizacao(), outra));
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().EncerrarAsync(30, outra));
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().CancelarAsync(30, outra));
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ListarInscritosAsync(30, outra));
        NaoSalvou();
    }

    [Fact]
    public async Task EncerrarECancelar()
    {
        var oportunidade = Existe(Fabrica.Oportunidade(30, Fabrica.Organizacao(20)));

        await Servico().EncerrarAsync(30, ComoOrganizacao(20));
        Assert.Equal(StatusOportunidade.Encerrada, oportunidade.Status);

        await Servico().CancelarAsync(30, ComoOrganizacao(20));
        Assert.Equal(StatusOportunidade.Cancelada, oportunidade.Status);
    }

    [Fact]
    public async Task ListarInscritos_DaDona()
    {
        var oportunidade = Existe(Fabrica.Oportunidade(30, Fabrica.Organizacao(20)));
        Inscricoes.Setup(r => r.ListarPorOportunidadeAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Fabrica.Inscricao(oportunidade: oportunidade)]);

        Assert.Single(await Servico().ListarInscritosAsync(30, ComoOrganizacao(20)));
    }

    [Fact]
    public async Task Recomendar_UsaOMotorEExcluiJaInscritas()
    {
        Existe(Fabrica.Cidadao(10, "Capão Redondo", AreaConhecimento.Tecnologia));
        Oportunidades.Setup(r => r.ListarDisponiveisAsync(Fabrica.Agora, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Fabrica.Oportunidade(1), Fabrica.Oportunidade(2), Fabrica.Oportunidade(3, area: AreaConhecimento.Beleza, bairro: "Centro")]);
        Inscricoes.Setup(r => r.ListarOportunidadesComInscricaoAtivaAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync([2L]);

        var recomendacoes = await Servico().RecomendarAsync(10, ComoCidadao(10), 5);

        var unica = Assert.Single(recomendacoes);
        Assert.Equal(1, unica.Oportunidade.Id);
        Assert.Equal(5, unica.Pontuacao);
        Assert.Equal(2, unica.Motivos.Count);
    }

    [Fact]
    public async Task Recomendar_ParaOutroCidadao_AcessoNegado_EInexistente_404()
    {
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().RecomendarAsync(10, ComoCidadao(11), 5));
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().RecomendarAsync(10, Admin, 5));
    }
}
