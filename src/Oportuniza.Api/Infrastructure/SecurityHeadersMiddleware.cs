namespace Oportuniza.Api.Infrastructure;

/// <summary>
/// Cabeçalhos de segurança em todas as respostas (recomendações OWASP para APIs REST):
/// impedem que o navegador "adivinhe" o tipo do conteúdo, que a resposta seja embutida em iframe
/// (clickjacking), que a URL vaze por Referer e que dados autenticados fiquem em cache.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    // O Swagger UI precisa carregar scripts e estilos próprios; o resto da API só devolve JSON.
    private const string CspApi = "default-src 'none'; frame-ancestors 'none'";
    private const string CspSwagger = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Content-Security-Policy"] = context.Request.Path.StartsWithSegments("/swagger") ? CspSwagger : CspApi;
            if (!context.Request.Path.StartsWithSegments("/swagger"))
                headers["Cache-Control"] = "no-store";
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            return Task.CompletedTask;
        });
        return next(context);
    }
}
