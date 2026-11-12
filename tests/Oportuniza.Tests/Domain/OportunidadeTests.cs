using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Domain;

public class OportunidadeTests
{
    [Fact]
    public void NovaOportunidade_ComecaAbertaESemVagasOcupadas()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 3);

        Assert.True(oportunidade.EstaAberta);
        Assert.Equal(3, oportunidade.VagasDisponiveis);
        Assert.True(oportunidade.TemVagas);
        Assert.Equal(20, oportunidade.OrganizacaoId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void VagasInvalidas_LancaValidacao(int vagas) =>
        Assert.Throws<ValidacaoException>(() => Fabrica.Oportunidade(vagas: vagas));

    [Fact]
    public void SemOrganizacao_OuSemPeriodo_LancaValidacao()
    {
        Assert.Throws<ValidacaoException>(() => new Oportunidade(null!, "T", "D", TipoOportunidade.Curso,
            Modalidade.Online, AreaConhecimento.Artes, "Centro", 1, 1, Fabrica.Periodo(), true));
        Assert.Throws<ValidacaoException>(() => new Oportunidade(Fabrica.Organizacao(), "T", "D", TipoOportunidade.Curso,
            Modalidade.Online, AreaConhecimento.Artes, "Centro", 1, 1, null!, true));
    }

    [Fact]
    public void CargaHorariaZero_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => new Oportunidade(Fabrica.Organizacao(), "T", "D", TipoOportunidade.Curso,
            Modalidade.Online, AreaConhecimento.Artes, "Centro", 0, 1, Fabrica.Periodo(), true));

    [Fact]
    public void Inscrever_ReservaVagaECriaInscricaoPendente()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 2);
        var cidadao = Fabrica.Cidadao();

        var inscricao = oportunidade.Inscrever(cidadao, Fabrica.Agora);

        Assert.Equal(StatusInscricao.Pendente, inscricao.Status);
        Assert.Same(cidadao, inscricao.Cidadao);
        Assert.Same(oportunidade, inscricao.Oportunidade);
        Assert.Equal(1, oportunidade.VagasDisponiveis);
    }

    [Fact]
    public void Inscrever_SemVagas_LancaValidacao_RN_I2()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 1);
        oportunidade.Inscrever(Fabrica.Cidadao(1), Fabrica.Agora);

        var erro = Assert.Throws<ValidacaoException>(() => oportunidade.Inscrever(Fabrica.Cidadao(2), Fabrica.Agora));
        Assert.Contains("vagas", erro.Message);
    }

    [Fact]
    public void Inscrever_Encerrada_LancaValidacao_RN_I1()
    {
        var oportunidade = Fabrica.Oportunidade();
        oportunidade.Encerrar();

        Assert.Throws<ValidacaoException>(() => oportunidade.Inscrever(Fabrica.Cidadao(), Fabrica.Agora));
    }

    [Fact]
    public void Inscrever_CidadaoInativo_LancaValidacao_RN_I4()
    {
        var cidadao = Fabrica.Cidadao();
        cidadao.Desativar();

        Assert.Throws<ValidacaoException>(() => Fabrica.Oportunidade().Inscrever(cidadao, Fabrica.Agora));
    }

    [Fact]
    public void Inscrever_DepoisDoInicio_LancaValidacao_RN_I5()
    {
        var oportunidade = Fabrica.Oportunidade(diasParaInicio: 1);

        Assert.Throws<ValidacaoException>(() => oportunidade.Inscrever(Fabrica.Cidadao(), Fabrica.Agora.AddDays(2)));
    }

    [Fact]
    public void Inscrever_CidadaoNulo_LancaValidacao() =>
        Assert.Throws<ValidacaoException>(() => Fabrica.Oportunidade().Inscrever(null!, Fabrica.Agora));

    [Fact]
    public void AceitaInscricoesEm_AbertaEAntesDoInicio()
    {
        var oportunidade = Fabrica.Oportunidade(diasParaInicio: 1);

        Assert.True(oportunidade.AceitaInscricoesEm(Fabrica.Agora));
        Assert.False(oportunidade.AceitaInscricoesEm(Fabrica.Agora.AddDays(2)));

        oportunidade.Cancelar();
        Assert.False(oportunidade.AceitaInscricoesEm(Fabrica.Agora));
    }

    [Fact]
    public void Atualizar_AlteraOsDados()
    {
        var oportunidade = Fabrica.Oportunidade();

        oportunidade.Atualizar("Novo título", "Nova descrição", Modalidade.Hibrida, AreaConhecimento.Artes,
            "Centro", 40, 50, Fabrica.Periodo(20));

        Assert.Equal("Novo título", oportunidade.Titulo);
        Assert.Equal(Modalidade.Hibrida, oportunidade.Modalidade);
        Assert.Equal(AreaConhecimento.Artes, oportunidade.Area);
        Assert.Equal("Centro", oportunidade.Bairro);
        Assert.Equal(40, oportunidade.CargaHoraria);
        Assert.Equal(50, oportunidade.VagasTotais);
    }

    [Fact]
    public void Atualizar_ComMenosVagasQueOcupadas_LancaValidacao()
    {
        var oportunidade = Fabrica.Oportunidade(vagas: 5);
        oportunidade.Inscrever(Fabrica.Cidadao(1), Fabrica.Agora);
        oportunidade.Inscrever(Fabrica.Cidadao(2), Fabrica.Agora);

        Assert.Throws<ValidacaoException>(() => oportunidade.Atualizar("T", "D", Modalidade.Online,
            AreaConhecimento.Tecnologia, "Centro", 10, 1, Fabrica.Periodo()));
    }

    [Fact]
    public void Atualizar_OportunidadeEncerrada_LancaValidacao()
    {
        var oportunidade = Fabrica.Oportunidade();
        oportunidade.Encerrar();

        Assert.Throws<ValidacaoException>(() => oportunidade.Atualizar("T", "D", Modalidade.Online,
            AreaConhecimento.Tecnologia, "Centro", 10, 10, Fabrica.Periodo()));
    }

    [Fact]
    public void Encerrar_DuasVezes_LancaValidacao()
    {
        var oportunidade = Fabrica.Oportunidade();
        oportunidade.Encerrar();

        Assert.Equal(StatusOportunidade.Encerrada, oportunidade.Status);
        Assert.Throws<ValidacaoException>(oportunidade.Encerrar);
    }

    [Fact]
    public void Cancelar_DepoisDeEncerrada_Permite_MasNaoDuasVezes()
    {
        var oportunidade = Fabrica.Oportunidade();
        oportunidade.Encerrar();
        oportunidade.Cancelar();

        Assert.Equal(StatusOportunidade.Cancelada, oportunidade.Status);
        Assert.Throws<ValidacaoException>(oportunidade.Cancelar);
    }

    [Fact]
    public void PertenceA_ComparaAOrganizacao()
    {
        var oportunidade = Fabrica.Oportunidade(organizacao: Fabrica.Organizacao(77));

        Assert.True(oportunidade.PertenceA(77));
        Assert.False(oportunidade.PertenceA(78));
    }
}
