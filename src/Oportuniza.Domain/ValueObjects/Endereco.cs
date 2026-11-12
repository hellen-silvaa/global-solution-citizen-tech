using System.Text.RegularExpressions;
using Oportuniza.Domain.Common;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Domain.ValueObjects;

/// <summary>
/// Value object de endereço. Imutável: para mudar, cria-se outro.
/// O bairro é a informação central do Oportuniza (recomendações e indicadores por bairro).
/// </summary>
public sealed partial class Endereco
{
    public string Cep { get; private set; } = null!;
    public string Logradouro { get; private set; } = null!;
    public string? Numero { get; private set; }
    public string Bairro { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Uf { get; private set; } = null!;

    private Endereco() { } // EF Core

    public Endereco(string cep, string logradouro, string? numero, string bairro, string cidade, string uf)
    {
        Cep = NormalizarCep(cep);
        Logradouro = Guard.Texto(logradouro, "Logradouro", 150);
        Numero = Guard.TextoOpcional(numero, "Número", 10);
        Bairro = Guard.Texto(bairro, "Bairro", 80);
        Cidade = Guard.Texto(cidade, "Cidade", 80);
        var ufLimpa = Guard.Texto(uf, "UF", 2).ToUpperInvariant();
        if (!UfRegex().IsMatch(ufLimpa))
            throw new ValidacaoException("UF deve ter 2 letras!");
        Uf = ufLimpa;
    }

    /// <summary>Remove o hífen e valida o formato 00000-000 / 00000000.</summary>
    public static string NormalizarCep(string? cep)
    {
        if (cep is null || !CepRegex().IsMatch(cep.Trim()))
            throw new ValidacaoException("CEP inválido!");
        return cep.Trim().Replace("-", "");
    }

    public bool MesmoBairro(string? bairro) =>
        string.Equals(Bairro, bairro?.Trim(), StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"^\d{5}-?\d{3}$")]
    private static partial Regex CepRegex();

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex UfRegex();
}
