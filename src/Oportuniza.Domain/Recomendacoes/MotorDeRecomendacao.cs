using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Domain.Recomendacoes;

/// <summary>Oportunidade recomendada com os motivos; a pontuação é a soma dos pesos.</summary>
public sealed record Recomendacao(Oportunidade Oportunidade, IReadOnlyList<MotivoRecomendacao> Motivos)
{
    public int Pontuacao => Motivos.Sum(m => m.Pontos);
}

/// <summary>
/// Serviço de domínio: ranqueia oportunidades para um cidadão aplicando todos os critérios registrados.
/// Não acessa banco: recebe as candidatas prontas e devolve o ranking (fácil de testar).
/// </summary>
public class MotorDeRecomendacao(IEnumerable<ICriterioRecomendacao> criterios)
{
    public const int LimiteMaximo = 20;

    private readonly IReadOnlyList<ICriterioRecomendacao> _criterios = criterios.ToList();

    public IReadOnlyList<Recomendacao> Recomendar(Cidadao cidadao, IEnumerable<Oportunidade> candidatas,
        IReadOnlySet<long> oportunidadesJaInscritas, DateTime agora, int limite)
    {
        return candidatas
            .Where(o => o.AceitaInscricoesEm(agora) && o.TemVagas && !oportunidadesJaInscritas.Contains(o.Id))
            .Select(o => new Recomendacao(o, Avaliar(cidadao, o)))
            .Where(r => r.Pontuacao > 0)
            .OrderByDescending(r => r.Pontuacao)
            .ThenBy(r => r.Oportunidade.Periodo.Inicio)
            .Take(Math.Clamp(limite, 1, LimiteMaximo))
            .ToList();
    }

    private List<MotivoRecomendacao> Avaliar(Cidadao cidadao, Oportunidade oportunidade) =>
        _criterios
            .Select(c => c.Avaliar(cidadao, oportunidade))
            .OfType<MotivoRecomendacao>()
            .ToList();
}
