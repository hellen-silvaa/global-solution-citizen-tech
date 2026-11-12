using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oportuniza.Infrastructure.Persistence;

namespace Oportuniza.Infrastructure.HealthChecks;

/// <summary>Verifica se o banco responde. Usado pelo endpoint /health e pelo monitoramento da infraestrutura.</summary>
public class BancoDeDadosHealthCheck(OportunizaDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Banco de dados acessível")
                : HealthCheckResult.Unhealthy("Banco de dados inacessível");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Banco de dados inacessível", e);
        }
    }
}
