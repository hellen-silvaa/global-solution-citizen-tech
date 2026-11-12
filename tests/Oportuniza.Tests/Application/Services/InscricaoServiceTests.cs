using Moq;
using Oportuniza.Application.Inscricoes.Regras;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Application.Services;

public class InscricaoServiceTests : CenarioDeServico
{
    private readonly Mock<IRegraInscricao> _regra = new();
    private readonly Cidadao _cidadao;
    private readonly Oportunidade _oportunidade;

    public InscricaoServiceTests()
    {
        _cidadao = Existe(Fabrica.Cidadao(10));
        _oportunidade = Existe(Fabrica.Oportunidade(30, Fabrica.Organizacao(20), vagas: 3));
    }

    private InscricaoService Servico() => new(Inscricoes.Object, Oportunidades.Object, Cidadaos.Object,
        [_regra.Object], UnitOfWork.Object, Relogio);

    private Inscricao InscricaoExistente() => Existe(Fabrica.Inscricao(40, _cidadao, _oportunidade));

    [Fact]
    public async Task Inscrever_ExecutaRegrasReservaVagaESalva()
    {
        var resultado = await Servico().InscreverAsync(30, ComoCidadao(10));

        Assert.Equal(StatusInscricao.Pendente, resultado.Status);
        Assert.Equal(2, _oportunidade.VagasDisponiveis);
        _regra.Verify(r => r.ValidarAsync(_cidadao, _oportunidade, It.IsAny<CancellationToken>()), Times.Once);
        Inscricoes.Verify(r => r.AdicionarAsync(It.IsAny<Inscricao>(), It.IsAny<CancellationToken>()), Times.Once);
        Salvou();
    }

    [Fact]
    public async Task Inscrever_RegraFalha_NaoReservaVagaNemSalva()
    {
        _regra.Setup(r => r.ValidarAsync(It.IsAny<Cidadao>(), It.IsAny<Oportunidade>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidacaoException("regra violada"));

        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().InscreverAsync(30, ComoCidadao(10)));

        Assert.Equal(3, _oportunidade.VagasDisponiveis);
        NaoSalvou();
    }

    [Fact]
    public async Task Inscrever_InvarianteDoAgregadoFalha_NaoSalva()
    {
        _oportunidade.Encerrar();
        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().InscreverAsync(30, ComoCidadao(10)));
        NaoSalvou();
    }

    [Fact]
    public async Task Inscrever_PerfilOrganizacao_AcessoNegado() =>
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().InscreverAsync(30, ComoOrganizacao(20)));

    [Fact]
    public async Task Inscrever_OportunidadeOuCidadaoInexistente_404()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().InscreverAsync(999, ComoCidadao(10)));
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().InscreverAsync(30, ComoCidadao(999)));
    }

    [Fact]
    public async Task Obter_CidadaoOrganizacaoEAdminVeem_TerceiroNao()
    {
        InscricaoExistente();

        Assert.Equal(40, (await Servico().ObterAsync(40, ComoCidadao(10))).Id);
        Assert.Equal(40, (await Servico().ObterAsync(40, ComoOrganizacao(20))).Id);
        Assert.Equal(40, (await Servico().ObterAsync(40, Admin)).Id);
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ObterAsync(40, ComoCidadao(11)));
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => Servico().ObterAsync(41, Admin));
    }

    [Fact]
    public async Task ConfirmarEConcluir_PelaOrganizacaoDona()
    {
        InscricaoExistente();

        Assert.Equal(StatusInscricao.Confirmada, (await Servico().ConfirmarAsync(40, ComoOrganizacao(20))).Status);
        Assert.Equal(StatusInscricao.Concluida, (await Servico().ConcluirAsync(40, Admin)).Status);
        Salvou(2);
    }

    [Fact]
    public async Task ConfirmarEConcluir_PeloProprioCidadaoOuOutraOrg_AcessoNegado()
    {
        InscricaoExistente();

        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ConfirmarAsync(40, ComoCidadao(10)));
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().ConcluirAsync(40, ComoOrganizacao(21)));
        NaoSalvou();
    }

    [Fact]
    public async Task Cancelar_PeloCidadao_LiberaVaga()
    {
        var inscricao = InscricaoExistente();

        await Servico().CancelarAsync(40, ComoCidadao(10));

        Assert.Equal(StatusInscricao.Cancelada, inscricao.Status);
        Assert.Equal(3, _oportunidade.VagasDisponiveis);
        Salvou();
    }

    [Fact]
    public async Task Cancelar_PorTerceiro_AcessoNegado()
    {
        InscricaoExistente();
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Servico().CancelarAsync(40, ComoCidadao(11)));
    }

    [Fact]
    public async Task Cancelar_DepoisDoInicio_LancaValidacao_RN_I6()
    {
        InscricaoExistente();
        Relogio.Agora = _oportunidade.Periodo.Inicio.AddHours(1);

        await Assert.ThrowsAsync<ValidacaoException>(() => Servico().CancelarAsync(40, ComoCidadao(10)));
        NaoSalvou();
    }
}
