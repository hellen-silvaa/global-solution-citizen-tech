using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Services;

namespace Oportuniza.Api.Controllers;

/// <summary>Ciclo de vida da inscrição: Pendente → Confirmada → Concluída (ou Cancelada).</summary>
[Route("inscricoes")]
[Tags("Inscrições")]
[Authorize]
public class InscricoesController(IInscricaoService inscricoes) : ApiControllerBase
{
    /// <summary>Detalha uma inscrição (o cidadão, a organização dona ou admin).</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<DadosDetalhamentoInscricao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoInscricao>> Detalhar(long id, CancellationToken ct) =>
        Ok(await inscricoes.ObterAsync(id, UsuarioLogado, ct));

    /// <summary>Organização confirma a participação (Pendente → Confirmada).</summary>
    [HttpPatch("{id:long}/confirmar")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType<DadosDetalhamentoInscricao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoInscricao>> Confirmar(long id, CancellationToken ct) =>
        Ok(await inscricoes.ConfirmarAsync(id, UsuarioLogado, ct));

    /// <summary>Organização registra a conclusão (Confirmada → Concluída). Alimenta o indicador de impacto.</summary>
    [HttpPatch("{id:long}/concluir")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType<DadosDetalhamentoInscricao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoInscricao>> Concluir(long id, CancellationToken ct) =>
        Ok(await inscricoes.ConcluirAsync(id, UsuarioLogado, ct));

    /// <summary>Cancela a inscrição antes do início e libera a vaga (RN-I6).</summary>
    [HttpPatch("{id:long}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        await inscricoes.CancelarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }
}
