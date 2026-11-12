using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Application.Common.Validation;

/// <summary>
/// Política de senha: de 8 a 64 caracteres, com pelo menos uma letra e um número,
/// e sem espaços no começo ou no fim (evita senha "invisível" diferente da digitada).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SenhaForteAttribute : ValidationAttribute
{
    public const int TamanhoMinimo = 8;
    public const int TamanhoMaximo = 64;

    public SenhaForteAttribute()
        : base($"Senha deve ter de {TamanhoMinimo} a {TamanhoMaximo} caracteres, com letras e números") { }

    public override bool IsValid(object? value) =>
        value is string senha
        && senha.Length is >= TamanhoMinimo and <= TamanhoMaximo
        && senha == senha.Trim()
        && senha.Any(char.IsLetter)
        && senha.Any(char.IsDigit);
}
