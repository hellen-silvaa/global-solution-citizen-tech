using System.Text.RegularExpressions;
using Oportuniza.Domain.Common;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Domain.ValueObjects;

/// <summary>
/// Value object de e-mail: sempre válido, sem espaços e em minúsculas.
/// Igualdade por valor (record), então "Ana@X.com" e "ana@x.com" são o mesmo e-mail.
/// </summary>
public sealed partial record Email
{
    public const int TamanhoMaximo = 120;

    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    public static Email Criar(string? valor)
    {
        var normalizado = Guard.Texto(valor, "E-mail", TamanhoMaximo).ToLowerInvariant();
        if (!Formato().IsMatch(normalizado))
            throw new ValidacaoException("E-mail inválido!");
        return new Email(normalizado);
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Formato();
}
