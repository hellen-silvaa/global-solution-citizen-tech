using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Oportuniza.Api.Infrastructure;

public static class HealthCheckResponse
{
    /// <summary>Corpo do /health: {"status":"UP","banco":"UP"}. Não expõe detalhes de erro.</summary>
    public static HealthCheckOptions Opcoes() => new()
    {
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        },
        ResponseWriter = (context, relatorio) =>
        {
            var banco = relatorio.Entries.TryGetValue("banco", out var entrada) && entrada.Status == HealthStatus.Healthy;
            return context.Response.WriteAsJsonAsync(new
            {
                status = relatorio.Status == HealthStatus.Healthy ? "UP" : "DOWN",
                banco = banco ? "UP" : "DOWN"
            });
        }
    };
}
