using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Recomendacoes;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Tests.Support;

namespace Oportuniza.Tests.Domain;

public class CriteriosRecomendacaoTests
{
    private readonly Cidadao _cidadao = Fabrica.Cidadao(10, "Capão Redondo", AreaConhecimento.Tecnologia);

    [Fact]
    public void Interesse_SoQuandoAAreaCombina()
    {
        var criterio = new CriterioInteresse();

        Assert.Equal(CriterioInteresse.Peso, criterio.Avaliar(_cidadao, Fabrica.Oportunidade(area: AreaConhecimento.Tecnologia))!.Pontos);
        Assert.Null(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(area: AreaConhecimento.Beleza)));
    }

    [Fact]
    public void MesmoBairro_SoQuandoOBairroCombina()
    {
        var criterio = new CriterioMesmoBairro();

        Assert.Equal(CriterioMesmoBairro.Peso, criterio.Avaliar(_cidadao, Fabrica.Oportunidade(bairro: "capão redondo"))!.Pontos);
        Assert.Null(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(bairro: "Centro")));
    }

    [Fact]
    public void Online_SoParaModalidadeOnline()
    {
        var criterio = new CriterioOnline();

        Assert.NotNull(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(modalidade: Modalidade.Online)));
        Assert.Null(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(modalidade: Modalidade.Hibrida)));
    }

    [Fact]
    public void Gratuita_SoParaGratuitas()
    {
        var criterio = new CriterioGratuita();

        Assert.NotNull(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(gratuita: true)));
        Assert.Null(criterio.Avaliar(_cidadao, Fabrica.Oportunidade(gratuita: false)));
    }
}

public class MotorDeRecomendacaoTests
{
    private static MotorDeRecomendacao Motor() =>
        new([new CriterioInteresse(), new CriterioMesmoBairro(), new CriterioOnline(), new CriterioGratuita()]);

    private readonly Cidadao _cidadao = Fabrica.Cidadao(10, "Capão Redondo", AreaConhecimento.Tecnologia);

    [Fact]
    public void Recomendar_OrdenaPorPontuacaoEExplicaMotivos()
    {
        var perfeita = Fabrica.Oportunidade(1, area: AreaConhecimento.Tecnologia, modalidade: Modalidade.Online); // 3+2+1+1
        var soInteresse = Fabrica.Oportunidade(2, area: AreaConhecimento.Tecnologia, bairro: "Centro", gratuita: false); // 3
        var semRelacao = Fabrica.Oportunidade(3, area: AreaConhecimento.Beleza, bairro: "Centro", gratuita: false); // 0

        var resultado = Motor().Recomendar(_cidadao, [semRelacao, soInteresse, perfeita], new HashSet<long>(), Fabrica.Agora, 5);

        Assert.Equal([1L, 2L], resultado.Select(r => r.Oportunidade.Id));
        Assert.Equal(7, resultado[0].Pontuacao);
        Assert.Equal(4, resultado[0].Motivos.Count);
        Assert.Equal(3, resultado[1].Pontuacao);
    }

    [Fact]
    public void Recomendar_ExcluiJaInscritasSemVagaEForaDoPrazo()
    {
        var jaInscrita = Fabrica.Oportunidade(1);
        var lotada = Fabrica.Oportunidade(2, vagas: 1);
        lotada.Inscrever(Fabrica.Cidadao(99), Fabrica.Agora);
        var jaComecou = Fabrica.Oportunidade(3, diasParaInicio: -1);
        var encerrada = Fabrica.Oportunidade(4);
        encerrada.Encerrar();
        var valida = Fabrica.Oportunidade(5);

        var resultado = Motor().Recomendar(_cidadao, [jaInscrita, lotada, jaComecou, encerrada, valida],
            new HashSet<long> { 1 }, Fabrica.Agora, 10);

        Assert.Equal([5L], resultado.Select(r => r.Oportunidade.Id));
    }

    [Fact]
    public void Recomendar_EmpateDesempataPelaDataDeInicio()
    {
        var depois = Fabrica.Oportunidade(1, diasParaInicio: 20);
        var antes = Fabrica.Oportunidade(2, diasParaInicio: 5);

        var resultado = Motor().Recomendar(_cidadao, [depois, antes], new HashSet<long>(), Fabrica.Agora, 10);

        Assert.Equal([2L, 1L], resultado.Select(r => r.Oportunidade.Id));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 2)]
    [InlineData(500, 25)]
    public void Recomendar_LimiteEntre1EOMaximo(int limite, int esperadoMaximo)
    {
        var candidatas = Enumerable.Range(1, 25).Select(i => Fabrica.Oportunidade(i)).ToList();

        var resultado = Motor().Recomendar(_cidadao, candidatas, new HashSet<long>(), Fabrica.Agora, limite);

        Assert.Equal(Math.Min(esperadoMaximo, MotorDeRecomendacao.LimiteMaximo), resultado.Count);
    }

    [Fact]
    public void Recomendar_SemCriterios_NaoRecomendaNada()
    {
        var resultado = new MotorDeRecomendacao([]).Recomendar(_cidadao, [Fabrica.Oportunidade()], new HashSet<long>(), Fabrica.Agora, 5);
        Assert.Empty(resultado);
    }

    [Fact]
    public void CriterioNovo_EntraSemMudarOMotor() // aberto/fechado
    {
        var motor = new MotorDeRecomendacao([new CriterioFixo()]);

        var resultado = motor.Recomendar(_cidadao, [Fabrica.Oportunidade()], new HashSet<long>(), Fabrica.Agora, 5);

        Assert.Equal(10, Assert.Single(resultado).Pontuacao);
    }

    private sealed class CriterioFixo : ICriterioRecomendacao
    {
        public MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade) => new(10, "Critério de teste");
    }
}
