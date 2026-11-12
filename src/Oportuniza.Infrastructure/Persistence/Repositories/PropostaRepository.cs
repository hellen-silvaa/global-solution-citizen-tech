using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Common;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Propostas;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

public class PropostaRepository(OportunizaDbContext db) : RepositorioBase<Proposta>(db), IPropostaRepository
{
    /// <summary>A proposta é carregada com autor e votos: as regras RN-P1/RN-P2 precisam dos votos.</summary>
    protected override IQueryable<Proposta> ConsultaDoAgregado =>
        Db.Propostas.Include(p => p.Autor).Include(p => p.Votos);

    public Task<PaginaResultado<Proposta>> ListarAsync(FiltroPropostas filtro, Paginacao paginacao, CancellationToken ct = default)
    {
        var query = ConsultaDoAgregado.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Bairro))
        {
            var bairro = filtro.Bairro.Trim();
            query = query.Where(p => p.Bairro == bairro);
        }
        if (filtro.Area.HasValue)
            query = query.Where(p => p.Area == filtro.Area.Value);
        if (filtro.Status.HasValue)
            query = query.Where(p => p.Status == filtro.Status.Value);

        // Ranking da comunidade: as propostas mais votadas aparecem primeiro.
        query = filtro.OrdenarPorVotos
            ? query.OrderByDescending(p => p.Votos.Count).ThenBy(p => p.CriadoEm).ThenBy(p => p.Id)
            : query.OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id);

        return query.AsSplitQuery().PaginarAsync(paginacao, ct);
    }
}
