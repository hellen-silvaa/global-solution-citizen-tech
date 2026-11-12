using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Oportuniza.Infrastructure.Persistence;

public static class MigracoesExtensions
{
    /// <summary>Aplica as migrações pendentes do EF Core (usado na subida da API em desenvolvimento).</summary>
    public static async Task AplicarMigracoesAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OportunizaDbContext>().Database.MigrateAsync(ct);
    }
}
