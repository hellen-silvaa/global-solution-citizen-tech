using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Api.Controllers;

/// <summary>Cursos, oficinas, mentorias e vagas de jovem aprendiz.</summary>
[Route("oportunidades")]
[Tags("Oportunidades")]
[Authorize]
public class OportunidadesController(IOportunidadeService oportunidades, IInscricaoService inscricoes) : ApiControllerBase
{
    /// <summary>Busca oportunidades (público), com filtros combináveis e paginação.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PaginaResultado<DadosDetalhamentoOportunidade>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaResultado<DadosDetalhamentoOportunidade>>> Buscar(
        [FromQuery] AreaConhecimento? area,
        [FromQuery] string? bairro,
        [FromQuery] Modalidade? modalidade,
        [FromQuery] TipoOportunidade? tipo,
        [FromQuery] bool? gratuita,
        [FromQuery] long? organizacaoId,
        [FromQuery] string? texto,
        [FromQuery] bool somenteComVagas = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        CancellationToken ct = default)
    {
        var filtro = new FiltroOportunidades(area, bairro, modalidade, tipo, gratuita, somenteComVagas, organizacaoId, texto);
        return Ok(await oportunidades.BuscarAsync(filtro, new Paginacao(pagina, tamanho), ct));
    }

    /// <summary>Detalha uma oportunidade (público).</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType<DadosDetalhamentoOportunidade>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoOportunidade>> Detalhar(long id, CancellationToken ct) =>
        Ok(await oportunidades.ObterAsync(id, ct));

    /// <summary>Publica uma oportunidade (perfil Organização).</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Organizacao)]
    [ProducesResponseType<DadosDetalhamentoOportunidade>(StatusCodes.Status201Created)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DadosDetalhamentoOportunidade>> Criar([FromBody] DadosCriacaoOportunidade dados, CancellationToken ct)
    {
        var oportunidade = await oportunidades.CriarAsync(dados, UsuarioLogado, ct);
        return CreatedAtAction(nameof(Detalhar), new { id = oportunidade.Id }, oportunidade);
    }

    /// <summary>Atualiza uma oportunidade aberta (RN-O1: só a organização dona ou admin).</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType<DadosDetalhamentoOportunidade>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DadosDetalhamentoOportunidade>> Atualizar(long id, [FromBody] DadosAtualizacaoOportunidade dados, CancellationToken ct) =>
        Ok(await oportunidades.AtualizarAsync(id, dados, UsuarioLogado, ct));

    /// <summary>Encerra as inscrições de uma oportunidade.</summary>
    [HttpPatch("{id:long}/encerrar")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Encerrar(long id, CancellationToken ct)
    {
        await oportunidades.EncerrarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }

    /// <summary>Cancela uma oportunidade.</summary>
    [HttpPatch("{id:long}/cancelar")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        await oportunidades.CancelarAsync(id, UsuarioLogado, ct);
        return NoContent();
    }

    /// <summary>Inscreve o cidadão logado na oportunidade (regras RN-I1 a RN-I5).</summary>
    [HttpPost("{id:long}/inscricoes")]
    [Authorize(Roles = Perfis.Cidadao)]
    [ProducesResponseType<DadosDetalhamentoInscricao>(StatusCodes.Status201Created)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DadosDetalhamentoInscricao>> Inscrever(long id, CancellationToken ct)
    {
        var inscricao = await inscricoes.InscreverAsync(id, UsuarioLogado, ct);
        return CreatedAtAction(nameof(InscricoesController.Detalhar), "Inscricoes", new { id = inscricao.Id }, inscricao);
    }

    /// <summary>Lista os inscritos (organização dona ou admin).</summary>
    [HttpGet("{id:long}/inscricoes")]
    [Authorize(Roles = $"{Perfis.Organizacao},{Perfis.Admin}")]
    [ProducesResponseType<IReadOnlyList<DadosDetalhamentoInscricao>>(StatusCodes.Status200OK)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<DadosErro>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DadosDetalhamentoInscricao>>> Inscritos(long id, CancellationToken ct) =>
        Ok(await oportunidades.ListarInscritosAsync(id, UsuarioLogado, ct));
}
