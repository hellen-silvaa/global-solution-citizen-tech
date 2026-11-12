using Oportuniza.Domain.Common;

namespace Oportuniza.Application.Ports.Persistence;

/// <summary>
/// Repositório genérico. A restrição <see cref="IRaizDeAgregado"/> impede, em tempo de compilação,
/// criar repositório para entidades internas de um agregado (ex.: Voto), como manda o DDD.
/// </summary>
public interface IRepositorio<T> where T : Entidade, IRaizDeAgregado
{
    Task<T?> ObterPorIdAsync(long id, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
}

/// <summary>Confirma as alterações rastreadas em uma única transação (uma por caso de uso).</summary>
public interface IUnitOfWork
{
    Task<int> SalvarAsync(CancellationToken ct = default);
}
