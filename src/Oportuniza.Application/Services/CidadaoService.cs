using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Inscricoes;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

public interface ICidadaoService
{
    Task<DadosDetalhamentoCidadao> CadastrarAsync(DadosCadastroCidadao dados, CancellationToken ct = default);
    Task<DadosDetalhamentoCidadao> ObterAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<PaginaResultado<DadosListagemCidadao>> ListarAsync(string? bairro, Paginacao paginacao, CancellationToken ct = default);
    Task<DadosDetalhamentoCidadao> AtualizarAsync(long id, DadosAtualizacaoCidadao dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task DesativarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task<IReadOnlyList<DadosDetalhamentoInscricao>> ListarInscricoesAsync(long id, StatusInscricao? status, UsuarioAutenticado usuario, CancellationToken ct = default);
}

public class CidadaoService(
    ICidadaoRepository cidadaos,
    IUsuarioRepository usuarios,
    IInscricaoRepository inscricoes,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork) : ICidadaoService
{
    private const string NaoEncontrado = "Cidadão não encontrado!";

    public async Task<DadosDetalhamentoCidadao> CadastrarAsync(DadosCadastroCidadao dados, CancellationToken ct = default)
    {
        var email = Email.Criar(dados.Email);
        if (await usuarios.EmailEmUsoAsync(email, ct))
            throw new ValidacaoException("E-mail já cadastrado!");

        var cidadao = new Cidadao(dados.Nome, email, hasher.Hash(dados.Senha), dados.Endereco.ParaValueObject(),
            dados.DataNascimento, dados.Escolaridade, dados.AreasInteresse);

        await cidadaos.AdicionarAsync(cidadao, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoCidadao.De(cidadao);
    }

    public async Task<DadosDetalhamentoCidadao> ObterAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        return DadosDetalhamentoCidadao.De(await cidadaos.ObterObrigatorioAsync(id, NaoEncontrado, ct));
    }

    public async Task<PaginaResultado<DadosListagemCidadao>> ListarAsync(string? bairro, Paginacao paginacao, CancellationToken ct = default) =>
        (await cidadaos.ListarAsync(bairro, paginacao, ct)).Mapear(DadosListagemCidadao.De);

    public async Task<DadosDetalhamentoCidadao> AtualizarAsync(long id, DadosAtualizacaoCidadao dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        var cidadao = await cidadaos.ObterObrigatorioAsync(id, NaoEncontrado, ct);

        cidadao.AtualizarDados(dados.Nome, dados.Endereco?.ParaValueObject());
        cidadao.AtualizarPerfil(dados.Escolaridade, dados.AreasInteresse);

        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoCidadao.De(cidadao);
    }

    public async Task DesativarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        var cidadao = await cidadaos.ObterObrigatorioAsync(id, NaoEncontrado, ct);
        cidadao.Desativar();
        await unitOfWork.SalvarAsync(ct);
    }

    public async Task<IReadOnlyList<DadosDetalhamentoInscricao>> ListarInscricoesAsync(
        long id, StatusInscricao? status, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        await cidadaos.ObterObrigatorioAsync(id, NaoEncontrado, ct);
        var lista = await inscricoes.ListarPorCidadaoAsync(id, status, ct);
        return lista.Select(DadosDetalhamentoInscricao.De).ToList();
    }
}
