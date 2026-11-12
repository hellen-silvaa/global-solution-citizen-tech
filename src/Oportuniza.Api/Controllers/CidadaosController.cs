using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Api.Controllers;

/// <summary>Cidadãos que procuram qualificação.</summary>
[Route("cidadaos")]
[Tags("Cidadãos")]
[Authorize]
public class CidadaosController(ICidadaoService cidadaos, IOportunidadeService oportunidades) : ApiControllerBase
{
    /// <summary>Cadastra um cidadão (público).</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<DadosDetalhamentoCidadao>(StatusCodes.Status201Created)]
    [ProducesResponseType<List<DadosErroValidacao>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DadosDetalhamentoCidadao>> Cadastrar([FromBody] DadosCadastroCidadao dados, CancellationToken ct)
    {
        var cidadao = await cidadaos.CadastrarAsync(dados, ct);
        return CreatedAtAction(nameof(Detalhar), new { id = cidadao.Id }, cidadao);
    }

    /// <summary>Lista cidadãos ativos (somente admin).</summary>
    [HttpGet]
    [Authorize(Roles = Perfis.Admin)]
    [ProducesResponseType<PaginaResultado<DadosListagemCidadao>>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginaResultado<DadosListagemCidadao>>> Listar(
        [FromQuery] string? bairro, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, CancellationToken ct = default) =>
        Ok(await cidadaos.ListarAsync(bairro, new Paginacao(pagina, tamanho), ct));

    /// <summary>Detalha um cidadão (o próprio ou admin).</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<DadosDetalhamentoCidadao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoCidadao>> Detalhar(long id, CancellationToken ct) =>
        Ok(await cidadaos.ObterAsync(id, UsuarioLogado, ct));

    /// <summary>Atualiza dados, interesses ou endereço (campos nulos não mudam).</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType<DadosDetalhamentoCidadao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoCidadao>> Atualizar(long id, [FromBody] DadosAtualizacaoCidadao dados, CancellationToken ct) =>
        Ok(await cidadaos.AtualizarAsync(id, dados, UsuarioLogado, ct));

    /// <summary>Desativa a conta (exclusão lógica: histórico preservado).</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(long id, CancellationToken ct)
    {
        await cidadaos.DesativarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }

    /// <summary>Inscrições do cidadão, opcionalmente filtradas por status.</summary>
    [HttpGet("{id:long}/inscricoes")]
    [ProducesResponseType<IReadOnlyList<DadosDetalhamentoInscricao>>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DadosDetalhamentoInscricao>>> Inscricoes(
        long id, [FromQuery] StatusInscricao? status, CancellationToken ct) =>
        Ok(await cidadaos.ListarInscricoesAsync(id, status, UsuarioLogado, ct));

    /// <summary>Oportunidades recomendadas, com pontuação e motivos.</summary>
    /// <remarks>Pontuação: interesse declarado (+3), mesmo bairro (+2), online (+1), gratuita (+1).</remarks>
    [HttpGet("{id:long}/recomendacoes")]
    [ProducesResponseType<IReadOnlyList<DadosRecomendacao>>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DadosRecomendacao>>> Recomendacoes(
        long id, [FromQuery] int limite = 5, CancellationToken ct = default) =>
        Ok(await oportunidades.RecomendarAsync(id, UsuarioLogado, limite, ct));
}
