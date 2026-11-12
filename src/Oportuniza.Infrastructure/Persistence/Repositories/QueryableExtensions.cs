using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Common;

namespace Oportuniza.Infrastructure.Persistence.Repositories;

internal static class QueryableExtensions
{
    /// <summary>Executa COUNT + página (Skip/Take) de forma assíncrona.</summary>
    public static async Task<PaginaResultado<T>> PaginarAsync<T>(this IQueryable<T> query, Paginacao paginacao, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var itens = await query
            .Skip(paginacao.Ignorar)
            .Take(paginacao.TamanhoNormalizado)
            .ToListAsync(ct);
        return new PaginaResultado<T>(itens, paginacao.PaginaNormalizada, paginacao.TamanhoNormalizado, total);
    }
}
