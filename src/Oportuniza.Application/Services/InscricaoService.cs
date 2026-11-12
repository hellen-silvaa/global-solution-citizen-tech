using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Inscricoes.Regras;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Inscricoes;

namespace Oportuniza.Application.Services;

public interface IInscricaoService
{
    Task<DadosDetalhamentoInscricao> InscreverAsync(long oportunidadeId, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<DadosDetalhamentoInscricao> ObterAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<DadosDetalhamentoInscricao> ConfirmarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<DadosDetalhamentoInscricao> ConcluirAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task CancelarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
}

public class InscricaoService(
    IInscricaoRepository inscricoes,
    IOportunidadeRepository oportunidades,
    ICidadaoRepository cidadaos,
    IEnumerable<IRegraInscricao> regras,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : IInscricaoService
{
    private const string NaoEncontrada = "Inscrição não encontrada!";

    private DateTime Agora => relogio.GetUtcNow().UtcDateTime;

    public async Task<DadosDetalhamentoInscricao> InscreverAsync(long oportunidadeId, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirPerfil(Perfil.Cidadao);
        var cidadao = await cidadaos.ObterObrigatorioAsync(usuario.Id, "Cidadão não encontrado!", ct);
        var oportunidade = await oportunidades.ObterObrigatorioAsync(oportunidadeId, "Oportunidade não encontrada!", ct);

        // Regras que consultam o banco (RN-I3, RN-I7)...
        foreach (var regra in regras)
            await regra.ValidarAsync(cidadao, oportunidade, ct);

        // ...e as invariantes do agregado (RN-I1, RN-I2, RN-I4, RN-I5).
        var inscricao = oportunidade.Inscrever(cidadao, Agora);

        await inscricoes.AdicionarAsync(inscricao, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoInscricao.De(inscricao);
    }

    public async Task<DadosDetalhamentoInscricao> ObterAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default) =>
        DadosDetalhamentoInscricao.De(await ObterEnvolvendoUsuarioAsync(id, usuario, ct));

    /// <summary>A organização confirma a participação do inscrito.</summary>
    public async Task<DadosDetalhamentoInscricao> ConfirmarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var inscricao = await ObterComoOrganizacaoAsync(id, usuario, ct);
        inscricao.Confirmar();
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoInscricao.De(inscricao);
    }

    /// <summary>A organização registra a conclusão: é o que alimenta o indicador de impacto.</summary>
    public async Task<DadosDetalhamentoInscricao> ConcluirAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var inscricao = await ObterComoOrganizacaoAsync(id, usuario, ct);
        inscricao.Concluir();
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoInscricao.De(inscricao);
    }

    /// <summary>RN-I6: o próprio cidadão, a organização ou um admin cancela antes do início.</summary>
    public async Task CancelarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        var inscricao = await ObterEnvolvendoUsuarioAsync(id, usuario, ct);
        inscricao.Cancelar(Agora);
        await unitOfWork.SalvarAsync(ct);
    }

    private async Task<Inscricao> ObterEnvolvendoUsuarioAsync(long id, UsuarioAutenticado usuario, CancellationToken ct)
    {
        var inscricao = await inscricoes.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        if (!usuario.EhAdmin && !inscricao.EnvolveUsuario(usuario.Id))
            throw new AcessoNegadoException();
        return inscricao;
    }

    private async Task<Inscricao> ObterComoOrganizacaoAsync(long id, UsuarioAutenticado usuario, CancellationToken ct)
    {
        var inscricao = await inscricoes.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        if (!usuario.EhAdmin && !inscricao.Oportunidade.PertenceA(usuario.Id))
            throw new AcessoNegadoException("Somente a organização responsável pode alterar esta inscrição!");
        return inscricao;
    }
}
