using Oportuniza.Application.Common;
using Oportuniza.Application.Dtos.Usuarios;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

public interface IOrganizacaoService
{
    Task<DadosDetalhamentoOrganizacao> CadastrarAsync(DadosCadastroOrganizacao dados, CancellationToken ct = default);
    Task<DadosDetalhamentoOrganizacao> ObterAsync(long id, CancellationToken ct = default);
    Task<PaginaResultado<DadosDetalhamentoOrganizacao>> ListarAsync(TipoOrganizacao? tipo, Paginacao paginacao, CancellationToken ct = default);
    Task<DadosDetalhamentoOrganizacao> AtualizarAsync(long id, DadosAtualizacaoOrganizacao dados, UsuarioAutenticado usuario, CancellationToken ct = default);
    Task DesativarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default);
}

public class OrganizacaoService(
    IOrganizacaoRepository organizacoes,
    IUsuarioRepository usuarios,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork) : IOrganizacaoService
{
    private const string NaoEncontrada = "Organização não encontrada!";

    public async Task<DadosDetalhamentoOrganizacao> CadastrarAsync(DadosCadastroOrganizacao dados, CancellationToken ct = default)
    {
        var email = Email.Criar(dados.Email);
        if (await usuarios.EmailEmUsoAsync(email, ct))
            throw new ValidacaoException("E-mail já cadastrado!");

        var organizacao = new Organizacao(dados.Nome, email, hasher.Hash(dados.Senha),
            dados.Endereco.ParaValueObject(), dados.Tipo, dados.Descricao, dados.Site);

        await organizacoes.AdicionarAsync(organizacao, ct);
        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoOrganizacao.De(organizacao);
    }

    public async Task<DadosDetalhamentoOrganizacao> ObterAsync(long id, CancellationToken ct = default) =>
        DadosDetalhamentoOrganizacao.De(await organizacoes.ObterObrigatorioAsync(id, NaoEncontrada, ct));

    public async Task<PaginaResultado<DadosDetalhamentoOrganizacao>> ListarAsync(TipoOrganizacao? tipo, Paginacao paginacao, CancellationToken ct = default) =>
        (await organizacoes.ListarAsync(tipo, paginacao, ct)).Mapear(DadosDetalhamentoOrganizacao.De);

    public async Task<DadosDetalhamentoOrganizacao> AtualizarAsync(long id, DadosAtualizacaoOrganizacao dados, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        var organizacao = await organizacoes.ObterObrigatorioAsync(id, NaoEncontrada, ct);

        organizacao.AtualizarDados(dados.Nome, dados.Endereco?.ParaValueObject());
        organizacao.AtualizarApresentacao(dados.Descricao, dados.Site);

        await unitOfWork.SalvarAsync(ct);
        return DadosDetalhamentoOrganizacao.De(organizacao);
    }

    public async Task DesativarAsync(long id, UsuarioAutenticado usuario, CancellationToken ct = default)
    {
        usuario.ExigirProprioOuAdmin(id);
        var organizacao = await organizacoes.ObterObrigatorioAsync(id, NaoEncontrada, ct);
        organizacao.Desativar();
        await unitOfWork.SalvarAsync(ct);
    }
}
