using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

public interface IEnderecoService
{
    Task<DadosEndereco> ConsultarCepAsync(string cep, CancellationToken ct = default);
}

/// <summary>Consulta o endereço de um CEP no WebService externo (ViaCEP) para preencher cadastros.</summary>
public class EnderecoService(IConsultaCepGateway gateway) : IEnderecoService
{
    public Task<DadosEndereco> ConsultarCepAsync(string cep, CancellationToken ct = default) =>
        gateway.ConsultarAsync(Endereco.NormalizarCep(cep), ct);
}
