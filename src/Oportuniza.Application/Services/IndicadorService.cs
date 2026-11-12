using Oportuniza.Application.Dtos.Indicadores;
using Oportuniza.Application.Ports.Queries;

namespace Oportuniza.Application.Services;

public interface IIndicadorService
{
    Task<DadosRelatorioImpacto> GerarRelatorioImpactoAsync(CancellationToken ct = default);
}

/// <summary>
/// Indicador de impacto da CitizenTech: quantas pessoas concluíram uma qualificação,
/// onde a oferta está e onde existe demanda sem oferta (propostas votadas e não atendidas).
/// </summary>
public class IndicadorService(IIndicadorQueries indicadores) : IIndicadorService
{
    public async Task<DadosRelatorioImpacto> GerarRelatorioImpactoAsync(CancellationToken ct = default)
    {
        var porBairro = await indicadores.ImpactoPorBairroAsync(ct);
        var porArea = await indicadores.DemandaPorAreaAsync(ct);

        var totalAtivas = porBairro.Sum(b => b.InscricoesAtivas);
        var totalConcluidas = porBairro.Sum(b => b.InscricoesConcluidas);
        var totalInscricoes = totalAtivas + totalConcluidas;
        var taxa = totalInscricoes == 0 ? 0 : Math.Round(100.0 * totalConcluidas / totalInscricoes, 1);

        // Demanda reprimida: áreas com propostas votadas, ordenadas pelas que têm menos oferta.
        var demanda = porArea
            .Where(a => a.PropostasAbertas > 0)
            .OrderByDescending(a => a.VotosEmPropostasAbertas)
            .ThenBy(a => a.OportunidadesAbertas)
            .ToList();

        return new DadosRelatorioImpacto(
            totalInscricoes,
            totalConcluidas,
            taxa,
            porBairro.Sum(b => b.PropostasAtendidas),
            porBairro.OrderByDescending(b => b.InscricoesConcluidas).ThenBy(b => b.Bairro).ToList(),
            demanda);
    }
}
