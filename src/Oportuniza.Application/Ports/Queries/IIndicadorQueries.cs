using Oportuniza.Application.Dtos.Indicadores;

namespace Oportuniza.Application.Ports.Queries;

/// <summary>
/// Lado de leitura (CQRS simplificado): consultas agregadas que devolvem DTOs direto do banco,
/// sem carregar agregados. Não altera estado, por isso não é um repositório.
/// </summary>
public interface IIndicadorQueries
{
    Task<IReadOnlyList<DadosIndicadorBairro>> ImpactoPorBairroAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DadosIndicadorArea>> DemandaPorAreaAsync(CancellationToken ct = default);
}
