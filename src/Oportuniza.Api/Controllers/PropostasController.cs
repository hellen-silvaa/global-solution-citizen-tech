using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Api.Controllers;

/// <summary>Participação cidadã: a comunidade pede qualificações para o bairro e vota.</summary>
[Route("propostas")]
[Tags("Propostas (participação cidadã)")]
[Authorize]
public class PropostasController(IPropostaService propostas) : ApiControllerBase
{
    /// <summary>Lista propostas (público). Por padrão, as mais votadas primeiro.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PaginaResultado<DadosDetalhamentoProposta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaResultado<DadosDetalhamentoProposta>>> Listar(
        [FromQuery] string? bairro,
        [FromQuery] AreaConhecimento? area,
        [FromQuery] StatusProposta? status,
        [FromQuery] bool ordenarPorVotos = true,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        CancellationToken ct = default) =>
        Ok(await propostas.ListarAsync(new FiltroPropostas(bairro, area, status, ordenarPorVotos), new Paginacao(pagina, tamanho), ct));

    /// <summary>Detalha uma proposta (público).</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType<DadosDetalhamentoProposta>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoProposta>> Detalhar(long id, CancellationToken ct) =>
        Ok(await propostas.ObterAsync(id, ct));

    /// <summary>Cria uma proposta (perfil Cidadão).</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Cidadao)]
    [ProducesResponseType<DadosDetalhamentoProposta>(StatusCodes.Status201Created)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DadosDetalhamentoProposta>> Criar([FromBody] DadosCriacaoProposta dados, CancellationToken ct)
    {
        var proposta = await propostas.CriarAsync(dados, UsuarioLogado, ct);
        return CreatedAtAction(nameof(Detalhar), new { id = proposta.Id }, proposta);
    }

    /// <summary>Vota em uma proposta (RN-P1: um voto por cidadão; RN-P2: só propostas abertas).</summary>
    [HttpPost("{id:long}/votos")]
    [Authorize(Roles = Perfis.Cidadao)]
    [ProducesResponseType<DadosDetalhamentoProposta>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoProposta>> Votar(long id, CancellationToken ct) =>
        Ok(await propostas.VotarAsync(id, UsuarioLogado, ct));

    /// <summary>Organização atende a proposta publicando uma oportunidade vinculada (RN-P3).</summary>
    [HttpPost("{id:long}/atender")]
    [Authorize(Roles = Perfis.Organizacao)]
    [ProducesResponseType<DadosDetalhamentoOportunidade>(StatusCodes.Status201Created)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoOportunidade>> Atender(long id, [FromBody] DadosCriacaoOportunidade dados, CancellationToken ct)
    {
        var oportunidade = await propostas.AtenderAsync(id, dados, UsuarioLogado, ct);
        return CreatedAtAction(nameof(OportunidadesController.Detalhar), "Oportunidades", new { id = oportunidade.Id }, oportunidade);
    }

    /// <summary>Arquiva a proposta (autor ou admin).</summary>
    [HttpPatch("{id:long}/arquivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Arquivar(long id, CancellationToken ct)
    {
        await propostas.ArquivarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }
}
