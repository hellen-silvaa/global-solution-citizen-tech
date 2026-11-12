using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Common;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

public class OportunidadeRepository(OportunizaDbContext db) : RepositorioBase<Oportunidade>(db), IOportunidadeRepository
{
    protected override IQueryable<Oportunidade> ConsultaDoAgregado => Db.Oportunidades.Include(o => o.Organizacao);

    /// <summary>
    /// Busca pública: cada filtro informado vira um Where na mesma consulta SQL.
    /// O texto livre é passado como parâmetro pelo EF Core (sem concatenação de SQL → sem SQL injection).
    /// </summary>
    public Task<PaginaResultado<Oportunidade>> BuscarAsync(FiltroOportunidades filtro, Paginacao paginacao, CancellationToken ct = default)
    {
        var query = ConsultaDoAgregado.AsNoTracking();

        if (filtro.Area.HasValue)
            query = query.Where(o => o.Area == filtro.Area.Value);
        if (filtro.Modalidade.HasValue)
            query = query.Where(o => o.Modalidade == filtro.Modalidade.Value);
        if (filtro.Tipo.HasValue)
            query = query.Where(o => o.Tipo == filtro.Tipo.Value);
        if (filtro.Gratuita.HasValue)
            query = query.Where(o => o.Gratuita == filtro.Gratuita.Value);
        if (filtro.OrganizacaoId.HasValue)
            query = query.Where(o => o.OrganizacaoId == filtro.OrganizacaoId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Bairro))
        {
            var bairro = filtro.Bairro.Trim();
            query = query.Where(o => o.Bairro == bairro);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            query = query.Where(o => o.Titulo.Contains(texto) || o.Descricao.Contains(texto));
        }
        if (filtro.SomenteComVagas)
            query = query.Where(o => o.Status == StatusOportunidade.Aberta && o.VagasOcupadas < o.VagasTotais);

        return query
            .OrderBy(o => o.Status)
            .ThenBy(o => o.Periodo.Inicio)
            .ThenBy(o => o.Id)
            .PaginarAsync(paginacao, ct);
    }

    public async Task<IReadOnlyList<Oportunidade>> ListarDisponiveisAsync(DateTime agora, CancellationToken ct = default) =>
        await ConsultaDoAgregado.AsNoTracking()
            .Where(o => o.Status == StatusOportunidade.Aberta
                        && o.VagasOcupadas < o.VagasTotais
                        && o.Periodo.Inicio > agora)
            .ToListAsync(ct);
}
