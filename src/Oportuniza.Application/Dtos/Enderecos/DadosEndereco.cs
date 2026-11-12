using System.ComponentModel.DataAnnotations;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Dtos.Enderecos;

/// <summary>Endereço usado no cadastro e devolvido pela consulta ao ViaCEP.</summary>
/// <param name="Cep">CEP com 8 dígitos, com ou sem hífen (ex.: 05767-000).</param>
/// <param name="Logradouro">Rua, avenida ou estrada.</param>
/// <param name="Numero">Número do imóvel (opcional).</param>
/// <param name="Bairro">Bairro. É usado para recomendar oportunidades perto do cidadão.</param>
/// <param name="Cidade">Cidade.</param>
/// <param name="Uf">Sigla do estado com 2 letras (ex.: SP).</param>
public record DadosEndereco(
    [Required(ErrorMessage = "CEP é obrigatório")] [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP inválido")] string Cep,
    [Required(ErrorMessage = "Logradouro é obrigatório")] [StringLength(150)] string Logradouro,
    [StringLength(10)] string? Numero,
    [Required(ErrorMessage = "Bairro é obrigatório")] [StringLength(80)] string Bairro,
    [Required(ErrorMessage = "Cidade é obrigatória")] [StringLength(80)] string Cidade,
    [Required(ErrorMessage = "UF é obrigatória")] [StringLength(2, MinimumLength = 2)] string Uf)
{
    public Endereco ParaValueObject() => new(Cep, Logradouro, Numero, Bairro, Cidade, Uf);

    public static DadosEndereco? De(Endereco? endereco) => endereco is null
        ? null
        : new DadosEndereco(endereco.Cep, endereco.Logradouro, endereco.Numero, endereco.Bairro, endereco.Cidade, endereco.Uf);
}
