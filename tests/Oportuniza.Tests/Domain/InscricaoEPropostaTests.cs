using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Propostas;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Domain;

public class InscricaoTests
{
    [Fact]
    public void FluxoCompleto_PendenteConfirmadaConcluida()
    {
        var inscricao = Fabrica.Inscricao();
        Assert.True(inscricao.EstaAtiva);

        inscricao.Confirmar();
        Assert.Equal(StatusInscricao.Confirmada, inscricao.Status);
        Assert.True(inscricao.EstaAtiva);

        inscricao.Concluir();
        Assert.Equal(StatusInscricao.Concluida, inscricao.Status);
        Assert.False(inscricao.EstaAtiva);
        Assert.NotNull(inscricao.AtualizadaEm);
    }

    [Fact]
    public void Concluir_SemConfirmar_LancaValidacao()
    {
        var erro = Assert.Throws<ValidacaoException>(Fabrica.Inscricao().Concluir);
        Assert.Contains("Pendente", erro.Message);
    }

    [Fact]
    public void Confirmar_DuasVezes_LancaValidacao()
    {
        var inscricao = Fabrica.Inscricao();
        inscricao.Confirmar();
        Assert.Throws<ValidacaoException>(inscricao.Confirmar);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancelar_PendenteOuConfirmada_LiberaAVaga(bool confirmarAntes)
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 1);
        var inscricao = Fabrica.Inscricao(oportunidade: oportunidade);
        if (confirmarAntes) inscricao.Confirmar();

        inscricao.Cancelar(Fabrica.Agora);

        Assert.Equal(StatusInscricao.Cancelada, inscricao.Status);
        Assert.Equal(1, oportunidade.VagasDisponiveis);
    }

    [Fact]
    public void Cancelar_DepoisDoInicio_LancaValidacao_RN_I6()
    {
        var inscricao = Fabrica.Inscricao();
        Assert.Throws<ValidacaoException>(() => inscricao.Cancelar(inscricao.Oportunidade.Periodo.Inicio));
    }

    [Fact]
    public void Cancelar_Concluida_LancaValidacaoENaoLiberaVaga()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 1);
        var inscricao = Fabrica.Inscricao(oportunidade: oportunidade);
        inscricao.Confirmar();
        inscricao.Concluir();

        Assert.Throws<ValidacaoException>(() => inscricao.Cancelar(Fabrica.Agora));
        Assert.Equal(0, oportunidade.VagasDisponiveis);
    }

    [Fact]
    public void Cancelar_DuasVezes_NaoLiberaVagaDuasVezes()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 2);
        var outra = Fabrica.Inscricao(41, Fabrica.Cidadao(1), oportunidade);
        var inscricao = Fabrica.Inscricao(42, Fabrica.Cidadao(2), oportunidade);
        inscricao.Cancelar(Fabrica.Agora);

        Assert.Throws<ValidacaoException>(() => inscricao.Cancelar(Fabrica.Agora));
        Assert.Equal(1, oportunidade.VagasDisponiveis);
        Assert.True(outra.EstaAtiva);
    }

    [Fact]
    public void Envolvimento_CidadaoEOrganizacaoSim_TerceiroNao()
    {
        var inscricao = Fabrica.Inscricao(cidadao: Fabrica.Cidadao(10), oportunidade: Fabrica.Oportunidade(organizacao: Fabrica.Organizacao(20)));

        Assert.True(inscricao.PertenceA(10));
        Assert.False(inscricao.PertenceA(20));
        Assert.True(inscricao.EnvolveUsuario(10));
        Assert.True(inscricao.EnvolveUsuario(20));
        Assert.False(inscricao.EnvolveUsuario(99));
    }
}

public class PropostaTests
{
    [Fact]
    public void NovaProposta_ComecaAbertaSemVotos()
    {
        var proposta = Fabrica.Proposta();

        Assert.True(proposta.EstaAberta);
        Assert.Equal(0, proposta.TotalVotos);
        Assert.Equal(10, proposta.AutorId);
    }

    [Fact]
    public void NovaProposta_AutorNuloOuInativo_LancaValidacao()
    {
        Assert.Throws<ValidacaoException>(() => new Proposta(null!, "T", "D", AreaConhecimento.Artes, "Centro"));

        var inativo = Fabrica.Cidadao();
        inativo.Desativar();
        Assert.Throws<ValidacaoException>(() => new Proposta(inativo, "T", "D", AreaConhecimento.Artes, "Centro"));
    }

    [Theory]
    [InlineData("", "D", "Centro")]
    [InlineData("T", "", "Centro")]
    [InlineData("T", "D", "")]
    public void NovaProposta_CamposVazios_LancaValidacao(string titulo, string descricao, string bairro) =>
        Assert.Throws<ValidacaoException>(() => new Proposta(Fabrica.Cidadao(), titulo, descricao, AreaConhecimento.Artes, bairro));

    [Fact]
    public void RegistrarVoto_ContabilizaEVinculaOVoto()
    {
        var proposta = Fabrica.Proposta();
        var eleitor = Fabrica.Cidadao(2);

        var voto = proposta.RegistrarVoto(eleitor);

        Assert.Equal(1, proposta.TotalVotos);
        Assert.Same(voto, Assert.Single(proposta.Votos));
        Assert.Same(proposta, voto.Proposta);
        Assert.Equal(2, voto.CidadaoId);
        Assert.Same(eleitor, voto.Cidadao);
    }

    [Fact]
    public void RegistrarVoto_MesmoCidadaoDuasVezes_LancaValidacao_RN_P1()
    {
        var proposta = Fabrica.Proposta();
        var cidadao = Fabrica.Cidadao(2);
        proposta.RegistrarVoto(cidadao);

        Assert.Throws<ValidacaoException>(() => proposta.RegistrarVoto(cidadao));
        Assert.Equal(1, proposta.TotalVotos);
    }

    [Fact]
    public void RegistrarVoto_PropostaAtendidaOuArquivada_LancaValidacao_RN_P2()
    {
        var atendida = Fabrica.Proposta();
        atendida.Atender(Fabrica.Oportunidade());
        var arquivada = Fabrica.Proposta();
        arquivada.Arquivar();

        Assert.Throws<ValidacaoException>(() => atendida.RegistrarVoto(Fabrica.Cidadao(2)));
        Assert.Throws<ValidacaoException>(() => arquivada.RegistrarVoto(Fabrica.Cidadao(2)));
    }

    [Fact]
    public void RegistrarVoto_CidadaoInativoOuNulo_LancaValidacao()
    {
        var inativo = Fabrica.Cidadao(2);
        inativo.Desativar();

        Assert.Throws<ValidacaoException>(() => Fabrica.Proposta().RegistrarVoto(inativo));
        Assert.Throws<ValidacaoException>(() => Fabrica.Proposta().RegistrarVoto(null!));
    }

    [Fact]
    public void Atender_VinculaOportunidade_EUmaVezSo()
    {
        var proposta = Fabrica.Proposta();
        var oportunidade = Fabrica.Oportunidade();

        proposta.Atender(oportunidade);

        Assert.Equal(StatusProposta.Atendida, proposta.Status);
        Assert.Same(oportunidade, proposta.OportunidadeGerada);
        Assert.Throws<ValidacaoException>(() => proposta.Atender(oportunidade));
        Assert.Throws<ValidacaoException>(() => Fabrica.Proposta().Atender(null!));
    }

    [Fact]
    public void Arquivar_SoPropostaAberta()
    {
        var proposta = Fabrica.Proposta();
        proposta.Arquivar();

        Assert.Equal(StatusProposta.Arquivada, proposta.Status);
        Assert.Throws<ValidacaoException>(proposta.Arquivar);
    }
}
