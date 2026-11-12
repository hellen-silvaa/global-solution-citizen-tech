namespace Oportuniza.Application.Common;

/// <summary>Página de resultados devolvida pelas listagens.</summary>
/// <param name="Itens">Itens da página atual.</param>
/// <param name="Pagina">Número da página (começa em 1).</param>
/// <param name="TamanhoPagina">Itens por página.</param>
/// <param name="TotalItens">Total de itens que atendem ao filtro, somando todas as páginas.</param>
public record PaginaResultado<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens)
{
    /// <summary>Total de páginas, calculado a partir do total de itens e do tamanho da página.</summary>
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);

    /// <summary>Converte os itens mantendo os dados de paginação.</summary>
    public PaginaResultado<TDestino> Mapear<TDestino>(Func<T, TDestino> conversor) =>
        new(Itens.Select(conversor).ToList(), Pagina, TamanhoPagina, TotalItens);
}

/// <summary>Parâmetros de paginação normalizados (página mínima 1, tamanho entre 1 e 50).</summary>
public record Paginacao(int Pagina = 1, int TamanhoPagina = 10)
{
    public int PaginaNormalizada => Math.Max(Pagina, 1);
    public int TamanhoNormalizado => Math.Clamp(TamanhoPagina, 1, 50);
    public int Ignorar => (PaginaNormalizada - 1) * TamanhoNormalizado;
}
