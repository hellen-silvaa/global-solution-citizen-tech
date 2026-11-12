using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Application.Dtos.Indicadores;
using Oportuniza.Application.Services;

namespace Oportuniza.Api.Controllers;

/// <summary>Indicador de impacto (transparência pública).</summary>
[Route("indicadores")]
[Tags("Indicadores")]
[AllowAnonymous]
public class IndicadoresController(IIndicadorService indicadores) : ApiControllerBase
{
    /// <summary>Relatório de impacto: conclusões, taxa de conclusão, oferta por bairro e demanda reprimida por área.</summary>
    [HttpGet("impacto")]
    [ProducesResponseType<DadosRelatorioImpacto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DadosRelatorioImpacto>> Impacto(CancellationToken ct) =>
        Ok(await indicadores.GerarRelatorioImpactoAsync(ct));
}
