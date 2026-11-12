using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Infrastructure.External.ViaCep;

/// <summary>
/// Adaptador do WebService REST público ViaCEP (https://viacep.com.br).
/// Implementa a porta <see cref="IConsultaCepGateway"/>; o HttpClient é configurado no DI
/// (URL base e timeout) e pode ser substituído nos testes.
/// </summary>
public partial class ViaCepClient(HttpClient http, ILogger<ViaCepClient> logger) : IConsultaCepGateway
{
    private const string Indisponivel = "Serviço de CEP indisponível no momento!";

    public async Task<DadosEndereco> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        ViaCepResponse? resposta;
        try
        {
            using var response = await http.GetAsync($"{cep}/json/", ct);
            if (!response.IsSuccessStatusCode)
            {
                // ViaCEP responde 400 para CEP mal formatado; tratamos como inexistente.
                if ((int)response.StatusCode < 500)
                    throw new RecursoNaoEncontradoException("CEP não encontrado!");
                throw new IntegracaoExternaException(Indisponivel);
            }
            resposta = await response.Content.ReadFromJsonAsync<ViaCepResponse>(ct);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            LogFalhaViaCep(logger, e, cep);
            throw new IntegracaoExternaException(Indisponivel, e);
        }

        if (resposta is null || resposta.CepInexistente || string.IsNullOrEmpty(resposta.Cep))
            throw new RecursoNaoEncontradoException("CEP não encontrado!");

        return new DadosEndereco(
            resposta.Cep.Replace("-", ""), resposta.Logradouro ?? "", null,
            resposta.Bairro ?? "", resposta.Localidade ?? "", resposta.Uf ?? "");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Falha ao consultar o ViaCEP para o CEP {Cep}")]
    private static partial void LogFalhaViaCep(ILogger logger, Exception exception, string cep);

    /// <summary>Formato da resposta do ViaCEP. Quando o CEP não existe, vem só {"erro": true}.</summary>
    private sealed class ViaCepResponse
    {
        [JsonPropertyName("cep")] public string? Cep { get; init; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; init; }
        [JsonPropertyName("bairro")] public string? Bairro { get; init; }
        [JsonPropertyName("localidade")] public string? Localidade { get; init; }
        [JsonPropertyName("uf")] public string? Uf { get; init; }

        // O ViaCEP já devolveu "erro" como booleano e como texto ("true"); aceitamos os dois.
        [JsonPropertyName("erro")] public object? Erro { get; init; }

        public bool CepInexistente => string.Equals(Erro?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }
}
