using Oportuniza.Application.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Oportunidades;

namespace Oportuniza.Application.Ports.Persistence;

/// <summary>Filtros da busca pública de oportunidades.</summary>
public record FiltroOportunidades(
    AreaConhecimento? Area = null,
    string? Bairro = null,
    Modalidade? Modalidade = null,
    TipoOportunidade? Tipo = null,
    bool? Gratuita = null,
    bool SomenteComVagas = false,
    long? OrganizacaoId = null,
    string? Texto = null);

public interface IOportunidadeRepository : IRepositorio<Oportunidade>
{
    Task<PaginaResultado<Oportunidade>> BuscarAsync(FiltroOportunidades filtro, Paginacao paginacao, CancellationToken ct = default);

    /// <summary>Oportunidades abertas, com vaga e que ainda aceitam inscrição (candidatas à recomendação).</summary>
    Task<IReadOnlyList<Oportunidade>> ListarDisponiveisAsync(DateTime agora, CancellationToken ct = default);
}
