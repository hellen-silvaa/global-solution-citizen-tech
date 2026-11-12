using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Recomendacoes;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

public interface IOportunidadeService
{
    Task<DadosDetalhamentoOportunidade> CriarAsync(DadosCriacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<PaginaResultado<DadosDetalhamentoOportunidade>> BuscarAsync(FiltroOportunidades filtro, Paginacao paginacao, CancellationToken ct = default);
    Task<DadosDetalhamentoOportunidade> ObterAsync(long id, CancellationToken ct = default);
    Task<DadosDetalhamentoOportunidade> AtualizarAsync(long id, DadosAtualizacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task EncerrarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task CancelarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<IReadOnlyList<DadosDetalhamentoInscricao>> ListarInscritosAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<IReadOnlyList<DadosRecomendacao>> RecomendarAsync(long cidadaoId, UsuarioAutenticado usuario, int limite, CancellationToken ct = default);
}

public class OportunidadeService(
    IOportunidadeRepository oportunidades,
    IOrganizacaoRepository organizacoes,
    ICidadaoRepository cidadaos,
    IInscricaoRepository inscricoes,
    MotorDeRecomendacao motorDeRecomendacao,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : IOportunidadeService
{
    private const string NaoEncontrada = "Oportunidade não encontrada!";

    private DateTime Agora => relogio.GetUtcNow().UtcDateTime;

    public async Task<DadosDetalhamentoOportunidade> CriarAsync(DadosCriacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirPerfil(Perfil.Organizacao);
        var organizacao = await organizacoes.ObterObrigatorioAsync(usuario.Id, "Organização não encontrada!", ct);

        var oportunidade = FabricaDeOportunidades.Criar(organizacao, dados, Agora);
        await oportunidades.AdicionarAsync(oportunidade, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoOportunidade.De(oportunidade);
    }

    public async Task<PaginaResultado<DadosDetalhamentoOportunidade>> BuscarAsync(FiltroOportunidades filtro, Paginacao paginacao, CancellationToken ct = default) =>
        (await oportunidades.BuscarAsync(filtro, paginacao, ct)).Mapear(DadosDetalhamentoOportunidade.De);

    public async Task<DadosDetalhamentoOportunidade> ObterAsync(long id, CancellationToken ct = default) =>
        DadosDetalhamentoOportunidade.De(await oportunidades.ObterObrigatorioAsync(id, NaoEncontrada, ct));

    public async Task<DadosDetalhamentoOportunidade> AtualizarAsync(long id, DadosAtualizacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var oportunidade = await ObterDaOrganizacaoAsync(id, usuario, ct);
        oportunidade.Atualizar(dados.Titulo, dados.Descricao, dados.Modalidade, dados.Area, dados.Bairro,
            dados.CargaHoraria, dados.Vagas, new Periodo(dados.DataInicio, dados.DataFim));
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoOportunidade.De(oportunidade);
    }

    public async Task EncerrarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var oportunidade = await ObterDaOrganizacaoAsync(id, usuario, ct);
        oportunidade.Encerrar();
        await unitOfWork.SalvarAsync(ct);
    }

    public async Task CancelarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var oportunidade = await ObterDaOrganizacaoAsync(id, usuario, ct);
        oportunidade.Cancelar();
        await unitOfWork.SalvarAsync(ct);
    }

    public async Task<IReadOnlyList<DadosDetalhamentoInscricao>> ListarInscritosAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        await ObterDaOrganizacaoAsync(id, usuario, ct);
        var lista = await inscricoes.ListarPorOportunidadeAsync(id, ct);
        return lista.Select(DadosDetalhamentoInscricao.De).ToList();
    }

    /// <summary>
    /// A pré-seleção (abertas, com vaga, no prazo) é feita no banco; a pontuação fica com o
    /// <see cref="MotorDeRecomendacao"/> do domínio.
    /// </summary>
    public async Task<IReadOnlyList<DadosRecomendacao>> RecomendarAsync(long cidadaoId, UsuarioAutenticado usuario, int limite, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(cidadaoId);
        var cidadao = await cidadaos.ObterObrigatorioAsync(cidadaoId, "Cidadão não encontrado!", ct);

        var candidatas = await oportunidades.ListarDisponiveisAsync(Agora, ct);
        var jaInscritas = (await inscricoes.ListarOportunidadesComInscricaoAtivaAsync(cidadaoId, ct)).ToHashSet();

        return motorDeRecomendacao
            .Recomendar(cidadao, candidatas, jaInscritas, Agora, limite)
            .Select(DadosRecomendacao.De)
            .ToList();
    }

    /// <summary>RN-O1: só a organização dona (ou um admin) altera a oportunidade.</summary>
    private async Task<Oportunidade> ObterDaOrganizacaoAsync(long id, UsuarioAutenticado usuario, CancellationToken ct)
    {
        var oportunidade = await oportunidades.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        if (!usuario.EhAdmin && !oportunidade.PertenceA(usuario.Id))
            throw new AcessoNegadoException("Somente a organização responsável pode alterar esta oportunidade!");
        return oportunidade;
    }
}
