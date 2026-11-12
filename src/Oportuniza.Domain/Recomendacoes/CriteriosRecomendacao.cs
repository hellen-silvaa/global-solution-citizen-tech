using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Domain.Recomendacoes;

/// <summary>Um motivo pelo qual a oportunidade foi recomendada, com o peso que ele soma.</summary>
public sealed record MotivoRecomendacao(int Pontos, string Descricao);

/// <summary>
/// Critério de recomendação (Strategy). Para criar um critério novo basta implementar esta interface
/// e registrá-la no DI: o <see cref="MotorDeRecomendacao"/> não muda (princípio aberto/fechado).
/// </summary>
public interface ICriterioRecomendacao
{
    /// <returns>O motivo, se o critério se aplica; senão, <c>null</c>.</returns>
    MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade);
}

/// <summary>Combina com uma área de interesse declarada pelo cidadão (+3).</summary>
public sealed class CriterioInteresse : ICriterioRecomendacao
{
    public const int Peso = 3;

    public MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade) =>
        cidadao.TemInteresseEm(oportunidade.Area)
            ? new MotivoRecomendacao(Peso, $"Combina com seu interesse em {oportunidade.Area}")
            : null;
}

/// <summary>Acontece no bairro do cidadão: sem custo de transporte (+2).</summary>
public sealed class CriterioMesmoBairro : ICriterioRecomendacao
{
    public const int Peso = 2;

    // Endereço é obrigatório para cidadão (invariante garantida no construtor de Cidadao).
    public MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade) =>
        cidadao.Endereco!.MesmoBairro(oportunidade.Bairro)
            ? new MotivoRecomendacao(Peso, $"Acontece no seu bairro ({oportunidade.Bairro})")
            : null;
}

/// <summary>É online: dá para fazer de qualquer lugar (+1).</summary>
public sealed class CriterioOnline : ICriterioRecomendacao
{
    public const int Peso = 1;

    public MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade) =>
        oportunidade.Modalidade == Modalidade.Online
            ? new MotivoRecomendacao(Peso, "É online: dá para fazer de casa")
            : null;
}

/// <summary>É gratuita (+1).</summary>
public sealed class CriterioGratuita : ICriterioRecomendacao
{
    public const int Peso = 1;

    public MotivoRecomendacao? Avaliar(Cidadao cidadao, Oportunidade oportunidade) =>
        oportunidade.Gratuita ? new MotivoRecomendacao(Peso, "É gratuita") : null;
}
