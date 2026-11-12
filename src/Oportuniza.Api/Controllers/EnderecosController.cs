using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Dtos.Enderecos;
using Oportuniza.Application.Services;

namespace Oportuniza.Api.Controllers;

/// <summary>Consulta de endereço no WebService externo ViaCEP.</summary>
[Route("enderecos")]
[Tags("Endereços (ViaCEP)")]
[AllowAnonymous]
public class EnderecosController(IEnderecoService enderecos) : ApiControllerBase
{
    /// <summary>Consulta um CEP no ViaCEP (usado para preencher o cadastro).</summary>
    [HttpGet("{cep}")]
    [ProducesResponseType<DadosEndereco>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DadosEndereco>> Consultar(string cep, CancellationToken ct) =>
        Ok(await enderecos.ConsultarCepAsync(cep, ct));
}
