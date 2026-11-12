using Microsoft.AspNetCore.Mvc;

namespace Oportuniza.Api.Infrastructure;

public static class ValidacaoResponse
{
    /// <summary>
    /// Erros de validação no formato [{ "campo": ..., "mensagem": ... }].
    /// Mensagens internas do desserializador (com nomes de tipos .NET) são trocadas por um texto neutro.
    /// </summary>
    public static IActionResult Criar(ActionContext context)
    {
        var erros = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(erro => new DadosErroValidacao(NomeDoCampo(e.Key), Mensagem(erro))))
            .ToList();
        return new BadRequestObjectResult(erros);
    }

    private static string Mensagem(Microsoft.AspNetCore.Mvc.ModelBinding.ModelError erro)
    {
        // Mensagens padrão do ASP.NET vêm em inglês e às vezes citam tipos internos: trocamos por textos neutros.
        var mensagem = erro.ErrorMessage;
        if (string.IsNullOrWhiteSpace(mensagem) || erro.Exception is not null
            || mensagem.Contains("JSON", StringComparison.Ordinal)
            || mensagem.Contains("not valid", StringComparison.OrdinalIgnoreCase)
            || mensagem.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            return "Valor inválido ou em formato incorreto";
        if (mensagem.Contains("request body is required", StringComparison.OrdinalIgnoreCase))
            return "Corpo da requisição é obrigatório";
        if (mensagem.EndsWith("field is required.", StringComparison.Ordinal))
            return "Campo obrigatório";
        return mensagem;
    }

    private static string NomeDoCampo(string chave)
    {
        var campo = chave.TrimStart('$', '.');
        return string.IsNullOrEmpty(campo) ? "corpo" : campo;
    }
}
