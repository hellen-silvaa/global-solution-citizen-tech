using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Dtos.Indicadores;
using Oportuniza.Application.Ports.Queries;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Infrastructure.Persistence.Queries;

/// <summary>
/// Consultas agregadas do indicador de impacto (lado de leitura). Cada GroupBy vira um GROUP BY no banco;
/// os resultados (poucas linhas, uma por bairro/área) são combinados em memória.
/// </summary>
public class IndicadorQueries(OportunizaDbContext db) : IIndicadorQueries
{
    public async Task<IReadOnlyList<DadosIndicadorBairro>> ImpactoPorBairroAsync(CancellationToken ct = default)
    {
        var oferta = await db.Oportunidades.AsNoTracking()
            .Where(o => o.Status == StatusOportunidade.Aberta)
            .GroupBy(o => o.Bairro)
            .Select(g => new { Bairro = g.Key, Abertas = g.Count(), Vagas = g.Sum(o => o.VagasTotais - o.VagasOcupadas) })
            .ToDictionaryAsync(x => x.Bairro, StringComparer.OrdinalIgnoreCase, ct);

        var inscricoes = await db.Inscricoes.AsNoTracking()
            .GroupBy(i => i.Oportunidade.Bairro)
            .Select(g => new
            {
                Bairro = g.Key,
                Ativas = g.Count(i => i.Status == StatusInscricao.Pendente || i.Status == StatusInscricao.Confirmada),
                Concluidas = g.Count(i => i.Status == StatusInscricao.Concluida)
            })
            .ToDictionaryAsync(x => x.Bairro, StringComparer.OrdinalIgnoreCase, ct);

        var propostas = await db.Propostas.AsNoTracking()
            .GroupBy(p => p.Bairro)
            .Select(g => new
            {
                Bairro = g.Key,
                Abertas = g.Count(p => p.Status == StatusProposta.Aberta),
                Atendidas = g.Count(p => p.Status == StatusProposta.Atendida)
            })
            .ToDictionaryAsync(x => x.Bairro, StringComparer.OrdinalIgnoreCase, ct);

        return oferta.Keys.Concat(inscricoes.Keys).Concat(propostas.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(bairro =>
            {
                var o = oferta.GetValueOrDefault(bairro);
                var i = inscricoes.GetValueOrDefault(bairro);
                var p = propostas.GetValueOrDefault(bairro);
                return new DadosIndicadorBairro(bairro,
                    o?.Abertas ?? 0, o?.Vagas ?? 0,
                    i?.Ativas ?? 0, i?.Concluidas ?? 0,
                    p?.Abertas ?? 0, p?.Atendidas ?? 0);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<DadosIndicadorArea>> DemandaPorAreaAsync(CancellationToken ct = default)
    {
        var oferta = await ContarPorAreaAsync(db.Oportunidades.Where(o => o.Status == StatusOportunidade.Aberta).Select(o => o.Area), ct);
        var propostas = await ContarPorAreaAsync(db.Propostas.Where(p => p.Status == StatusProposta.Aberta).Select(p => p.Area), ct);
        var votos = await ContarPorAreaAsync(db.Votos.Where(v => v.Proposta.Status == StatusProposta.Aberta).Select(v => v.Proposta.Area), ct);

        return Enum.GetValues<AreaConhecimento>()
            .Select(area => new DadosIndicadorArea(area,
                oferta.GetValueOrDefault(area), propostas.GetValueOrDefault(area), votos.GetValueOrDefault(area)))
            .ToList();
    }

    private static Task<Dictionary<AreaConhecimento, int>> ContarPorAreaAsync(IQueryable<AreaConhecimento> areas, CancellationToken ct) =>
        areas.GroupBy(a => a)
            .Select(g => new { Area = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Area, x => x.Total, ct);
}
