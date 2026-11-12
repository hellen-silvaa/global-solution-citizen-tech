using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Application.Services;

namespace Oportuniza.Api.Controllers;

/// <summary>Autenticação.</summary>
[Route("auth")]
[Tags("Autenticação")]
public class AuthController(IAutenticacaoService autenticacao) : ApiControllerBase
{
    /// <summary>Faz login e devolve o token JWT.</summary>
    /// <remarks>
    /// Use o token no botão **Authorize** do Swagger. Proteções: limite de requisições por IP
    /// e bloqueio da conta por 15 minutos após 5 senhas erradas seguidas (429).
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingConfiguration.PoliticaLogin)]
    [ProducesResponseType<DadosToken>(StatusCodes.Status200OK)]
    [ProducesResponseType<List<DadosErroValidacao>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DadosToken>> Login([FromBody] DadosLogin dados, CancellationToken ct) =>
        Ok(await autenticacao.LoginAsync(dados, ct));
}
