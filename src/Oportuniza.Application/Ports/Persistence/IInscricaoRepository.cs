using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Inscricoes;

namespace Oportuniza.Application.Ports.Persistence;

public interface IInscricaoRepository : IRepositorio<Inscricao>
{
    Task<bool> ExisteInscricaoAtivaAsync(long cidadaoId, long oportunidadeId, CancellationToken ct = default);
    Task<int> ContarAtivasDoCidadaoAsync(long cidadaoId, CancellationToken ct = default);
    Task<IReadOnlyList<long>> ListarOportunidadesComInscricaoAtivaAsync(long cidadaoId, CancellationToken ct = default);
    Task<IReadOnlyList<Inscricao>> ListarPorCidadaoAsync(long cidadaoId, StatusInscricao? status, CancellationToken ct = default);
    Task<IReadOnlyList<Inscricao>> ListarPorOportunidadeAsync(long oportunidadeId, CancellationToken ct = default);
}
