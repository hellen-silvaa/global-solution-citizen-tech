using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Domain.Common;

/// <summary>Validações de invariantes usadas pelos construtores e métodos do domínio.</summary>
internal static class Guard
{
    public static string Texto(string? valor, string campo, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValidacaoException($"{campo} é obrigatório!");
        var limpo = valor.Trim();
        if (limpo.Length > max)
            throw new ValidacaoException($"{campo} deve ter no máximo {max} caracteres!");
        return limpo;
    }

    public static string? TextoOpcional(string? valor, string campo, int max) =>
        string.IsNullOrWhiteSpace(valor) ? null : Texto(valor, campo, max);

    public static int Positivo(int valor, string campo)
    {
        if (valor <= 0)
            throw new ValidacaoException($"{campo} deve ser maior que zero!");
        return valor;
    }

    public static T NaoNulo<T>(T? valor, string campo) where T : class =>
        valor ?? throw new ValidacaoException($"{campo} é obrigatório!");
}
