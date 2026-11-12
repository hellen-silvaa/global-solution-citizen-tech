using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Inscricoes;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

public class InscricaoRepository(OportunizaDbContext db) : RepositorioBase<Inscricao>(db), IInscricaoRepository
{
    private static readonly StatusInscricao[] StatusAtivos = [StatusInscricao.Pendente, StatusInscricao.Confirmada];

    protected override IQueryable<Inscricao> ConsultaDoAgregado =>
        Db.Inscricoes.Include(i => i.Cidadao).Include(i => i.Oportunidade);

    private IQueryable<Inscricao> AtivasDoCidadao(long cidadaoId) =>
        Db.Inscricoes.Where(i => i.CidadaoId == cidadaoId && StatusAtivos.Contains(i.Status));

    public Task<bool> ExisteInscricaoAtivaAsync(long cidadaoId, long oportunidadeId, CancellationToken ct = default) =>
        AtivasDoCidadao(cidadaoId).AnyAsync(i => i.OportunidadeId == oportunidadeId, ct);

    public Task<int> ContarAtivasDoCidadaoAsync(long cidadaoId, CancellationToken ct = default) =>
        AtivasDoCidadao(cidadaoId).CountAsync(ct);

    public async Task<IReadOnlyList<long>> ListarOportunidadesComInscricaoAtivaAsync(long cidadaoId, CancellationToken ct = default) =>
        await AtivasDoCidadao(cidadaoId).Select(i => i.OportunidadeId).ToListAsync(ct);

    public async Task<IReadOnlyList<Inscricao>> ListarPorCidadaoAsync(long cidadaoId, StatusInscricao? status, CancellationToken ct = default)
    {
        var query = Db.Inscricoes.AsNoTracking()
            .Include(i => i.Oportunidade)
            .Where(i => i.CidadaoId == cidadaoId);
        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);
        return await query.OrderByDescending(i => i.CriadoEm).ThenByDescending(i => i.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Inscricao>> ListarPorOportunidadeAsync(long oportunidadeId, CancellationToken ct = default) =>
        await Db.Inscricoes.AsNoTracking()
            .Include(i => i.Cidadao)
            .Where(i => i.OportunidadeId == oportunidadeId)
            .OrderBy(i => i.Status)
            .ThenBy(i => i.Cidadao.Nome)
            .ToListAsync(ct);
}
