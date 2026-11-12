using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Oportunidades;
using Oportuniza.Application.Dtos.Propostas;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Propostas;

namespace Oportuniza.Application.Services;

public interface IPropostaService
{
    Task<DadosDetalhamentoProposta> CriarAsync(DadosCriacaoProposta dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<PaginaResultado<DadosDetalhamentoProposta>> ListarAsync(FiltroPropostas filtro, Paginacao paginacao, CancellationToken ct = default);
    Task<DadosDetalhamentoProposta> ObterAsync(long id, CancellationToken ct = default);
    Task<DadosDetalhamentoProposta> VotarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<DadosDetalhamentoOportunidade> AtenderAsync(long id, DadosCriacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task ArquivarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
}

/// <summary>
/// Ciclo de participação cidadã: o cidadão propõe → a comunidade vota → uma organização atende
/// publicando uma oportunidade vinculada à proposta.
/// </summary>
public class PropostaService(
    IPropostaRepository propostas,
    ICidadaoRepository cidadaos,
    IOrganizacaoRepository organizacoes,
    IOportunidadeRepository oportunidades,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : IPropostaService
{
    private const string NaoEncontrada = "Proposta não encontrada!";
    private const string CidadaoNaoEncontrado = "Cidadão não encontrado!";

    public async Task<DadosDetalhamentoProposta> CriarAsync(DadosCriacaoProposta dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirPerfil(Perfil.Cidadao);
        var autor = await cidadaos.ObterObrigatorioAsync(usuario.Id, CidadaoNaoEncontrado, ct);

        var proposta = new Proposta(autor, dados.Titulo, dados.Descricao, dados.Area, dados.Bairro);
        await propostas.AdicionarAsync(proposta, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoProposta.De(proposta);
    }

    public async Task<PaginaResultado<DadosDetalhamentoProposta>> ListarAsync(FiltroPropostas filtro, Paginacao paginacao, CancellationToken ct = default) =>
        (await propostas.ListarAsync(filtro, paginacao, ct)).Mapear(DadosDetalhamentoProposta.De);

    public async Task<DadosDetalhamentoProposta> ObterAsync(long id, CancellationToken ct = default) =>
        DadosDetalhamentoProposta.De(await propostas.ObterObrigatorioAsync(id, NaoEncontrada, ct));

    /// <summary>RN-P1 e RN-P2 ficam no agregado (<see cref="Proposta.RegistrarVoto"/>).</summary>
    public async Task<DadosDetalhamentoProposta> VotarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirPerfil(Perfil.Cidadao);
        var proposta = await propostas.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        var cidadao = await cidadaos.ObterObrigatorioAsync(usuario.Id, CidadaoNaoEncontrado, ct);

        proposta.RegistrarVoto(cidadao);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoProposta.De(proposta);
    }

    /// <summary>RN-P3: só organizações atendem propostas; a oportunidade nasce vinculada.</summary>
    public async Task<DadosDetalhamentoOportunidade> AtenderAsync(long id, DadosCriacaoOportunidade dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirPerfil(Perfil.Organizacao);
        var proposta = await propostas.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        var organizacao = await organizacoes.ObterObrigatorioAsync(usuario.Id, "Organização não encontrada!", ct);

        var oportunidade = FabricaDeOportunidades.Criar(organizacao, dados, relogio.GetUtcNow().UtcDateTime);
        proposta.Atender(oportunidade);

        await oportunidades.AdicionarAsync(oportunidade, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoOportunidade.De(oportunidade);
    }

    /// <summary>O autor ou um admin retira a proposta da votação.</summary>
    public async Task ArquivarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var proposta = await propostas.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        usuario.ExigirProprioOuAdmin(proposta.AutorId);
        proposta.Arquivar();
        await unitOfWork.SalvarAsync(ct);
    }
}
