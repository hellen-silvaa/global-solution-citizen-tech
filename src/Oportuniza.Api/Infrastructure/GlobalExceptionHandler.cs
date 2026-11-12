using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Api.Infrastructure;

/// <summary>Corpo padrão de erro da API.</summary>
/// <param name="Mensagem">Explicação do erro em português, sem detalhes internos.</param>
public record DadosErro(string Mensagem);

/// <summary>Erro de validação de um campo da requisição.</summary>
/// <param name="Campo">Nome do campo inválido.</param>
/// <param name="Mensagem">O que está errado nesse campo.</param>
public record DadosErroValidacao(string Campo, string Mensagem);

/// <summary>
/// Tratamento centralizado de exceções: traduz cada categoria de exceção para o status HTTP correspondente.
/// Erros inesperados viram 500 com mensagem neutra: detalhes internos (stack trace, SQL) nunca vão para o cliente.
/// </summary>
public partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, mensagem) = Mapear(exception);

        if (status == StatusCodes.Status500InternalServerError)
            LogErroInesperado(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        else if (status >= StatusCodes.Status409Conflict)
            LogFalhaTratada(logger, status, httpContext.Request.Path, mensagem);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new DadosErro(mensagem), cancellationToken);
        return true;
    }

    public static (int Status, string Mensagem) Mapear(Exception exception) => exception switch
    {
        ValidacaoException e => (StatusCodes.Status400BadRequest, e.Message),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida!"),
        CredenciaisInvalidasException e => (StatusCodes.Status401Unauthorized, e.Message),
        AcessoNegadoException e => (StatusCodes.Status403Forbidden, e.Message),
        RecursoNaoEncontradoException e => (StatusCodes.Status404NotFound, e.Message),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
            "O registro foi alterado por outra pessoa ao mesmo tempo. Tente novamente!"),
        DbUpdateException => (StatusCodes.Status409Conflict, "Dados informados conflitam com registros existentes!"),
        MuitasTentativasException e => (StatusCodes.Status429TooManyRequests, e.Message),
        IntegracaoExternaException e => (StatusCodes.Status503ServiceUnavailable, e.Message),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor!")
    };

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro inesperado em {Metodo} {Caminho}")]
    private static partial void LogErroInesperado(ILogger logger, Exception exception, string metodo, string caminho);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Falha tratada ({Status}) em {Caminho}: {Mensagem}")]
    private static partial void LogFalhaTratada(ILogger logger, int status, string caminho, string mensagem);
}
