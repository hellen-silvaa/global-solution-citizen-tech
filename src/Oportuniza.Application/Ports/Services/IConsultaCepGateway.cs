using Oportuniza.Application.Dtos.Enderecos;

namespace Oportuniza.Application.Ports.Services;

/// <summary>Porta para o WebService externo de CEP (implementada pelo ViaCepClient).</summary>
public interface IConsultaCepGateway
{
    /// <exception cref="Domain.Exceptions.RecursoNaoEncontradoException">CEP inexistente.</exception>
    /// <exception cref="Domain.Exceptions.IntegracaoExternaException">Serviço fora do ar.</exception>
    Task<DadosEndereco> ConsultarAsync(string cep, CancellationToken ct = default);
}
