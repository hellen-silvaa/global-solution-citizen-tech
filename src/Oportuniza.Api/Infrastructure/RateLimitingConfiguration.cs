using System.Threading.RateLimiting;

namespace Oportuniza.Api.Infrastructure;

/// <summary>
/// Limite de requisições por IP (proteção contra força bruta e abuso):
/// um limite global para toda a API e um mais rígido para o login.
/// Os valores vêm da seção "RateLimit" da configuração.
/// </summary>
public static class RateLimitingConfiguration
{
    public const string PoliticaLogin = "login";

    public static IServiceCollection AddLimitesDeRequisicao(this IServiceCollection services, IConfiguration configuration)
    {
        var porMinuto = configuration.GetValue("RateLimit:RequisicoesPorMinuto", 300);
        var loginPorMinuto = configuration.GetValue("RateLimit:LoginPorMinuto", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new DadosErro("Muitas requisições. Aguarde um minuto e tente novamente!"), ct);

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(ChaveDoCliente(context), _ => Janela(porMinuto)));

            options.AddPolicy(PoliticaLogin, context =>
                RateLimitPartition.GetFixedWindowLimiter(ChaveDoCliente(context), _ => Janela(loginPorMinuto)));
        });
        return services;
    }

    /// <summary>Particiona o limite por IP do cliente; sem IP conhecido, todos caem no mesmo balde.</summary>
    internal static string ChaveDoCliente(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    private static FixedWindowRateLimiterOptions Janela(int limite) => new()
    {
        PermitLimit = limite,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    };
}
