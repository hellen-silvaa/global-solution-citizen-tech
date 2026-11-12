using Oportuniza.Application.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Propostas;

namespace Oportuniza.Application.Ports.Persistence;

/// <summary>Filtros da listagem de propostas da comunidade.</summary>
public record FiltroPropostas(
    string? Bairro = null,
    AreaConhecimento? Area = null,
    StatusProposta? Status = null,
    bool OrdenarPorVotos = true);

public interface IPropostaRepository : IRepositorio<Proposta>
{
    Task<PaginaResultado<Proposta>> ListarAsync(FiltroPropostas filtro, Paginacao paginacao, CancellationToken ct = default);
}
