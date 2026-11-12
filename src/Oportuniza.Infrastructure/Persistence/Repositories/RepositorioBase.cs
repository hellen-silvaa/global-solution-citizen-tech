using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Common;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação comum de <see cref="IRepositorio{T}"/>. Cada repositório concreto só redefine
/// <see cref="ConsultaDoAgregado"/> para dizer o que precisa ser carregado junto com a raiz.
/// </summary>
public abstract class RepositorioBase<T>(OportunizaDbContext db) : IRepositorio<T>
    where T : Entidade, IRaizDeAgregado
{
    protected OportunizaDbContext Db { get; } = db;

    /// <summary>Consulta usada para carregar o agregado completo (com os Includes necessários).</summary>
    protected virtual IQueryable<T> ConsultaDoAgregado => Db.Set<T>();

    public Task<T?> ObterPorIdAsync(long id, CancellationToken ct = default) =>
        ConsultaDoAgregado.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default) =>
        await Db.Set<T>().AddAsync(entidade, ct);
}
