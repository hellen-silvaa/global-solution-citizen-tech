using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Api.Controllers;

/// <summary>Organizações que oferecem qualificação (ONGs, escolas técnicas, empresas, poder público).</summary>
[Route("organizacoes")]
[Tags("Organizações")]
[Authorize]
public class OrganizacoesController(IOrganizacaoService organizacoes) : ApiControllerBase
{
    /// <summary>Cadastra uma organização (público).</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<DadosDetalhamentoOrganizacao>(StatusCodes.Status201Created)]
    [ProducesResponseType<List<DadosErroValidacao>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DadosDetalhamentoOrganizacao>> Cadastrar([FromBody] DadosCadastroOrganizacao dados, CancellationToken ct)
    {
        var organizacao = await organizacoes.CadastrarAsync(dados, ct);
        return CreatedAtAction(nameof(Detalhar), new { id = organizacao.Id }, organizacao);
    }

    /// <summary>Lista organizações ativas (público).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PaginaResultado<DadosDetalhamentoOrganizacao>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaResultado<DadosDetalhamentoOrganizacao>>> Listar(
        [FromQuery] TipoOrganizacao? tipo, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, CancellationToken ct = default) =>
        Ok(await organizacoes.ListarAsync(tipo, new Paginacao(pagina, tamanho), ct));

    /// <summary>Detalha uma organização (público).</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType<DadosDetalhamentoOrganizacao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoOrganizacao>> Detalhar(long id, CancellationToken ct) =>
        Ok(await organizacoes.ObterAsync(id, ct));

    /// <summary>Atualiza a organização (ela mesma ou admin).</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType<DadosDetalhamentoOrganizacao>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoOrganizacao>> Atualizar(long id, [FromBody] DadosAtualizacaoOrganizacao dados, CancellationToken ct) =>
        Ok(await organizacoes.AtualizarAsync(id, dados, UsuarioLogado, ct));

    /// <summary>Desativa a organização (exclusão lógica).</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(long id, CancellationToken ct)
    {
        await organizacoes.DesativarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }
}
