using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Exceptions;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Application.Services;

public interface IAutenticacaoService
{
    Task<DadosToken> LoginAsync(DadosLogin dados, CancellationToken ct = default);
}

/// <summary>
/// Login com três proteções:
/// 1) a mesma mensagem para e-mail inexistente, senha errada ou conta inativa (não revela quais e-mails existem);
/// 2) tempo de resposta constante: o hash é verificado mesmo quando o usuário não existe;
/// 3) bloqueio temporário da conta após 5 senhas erradas seguidas (força bruta).
/// </summary>
public class AutenticacaoService(
    IUsuarioRepository usuarios,
    IPasswordHasher hasher,
    ITokenService tokens,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : IAutenticacaoService
{
    public async Task<DadosToken> LoginAsync(DadosLogin dados, CancellationToken ct = default)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var usuario = await BuscarUsuarioAsync(dados.Email, ct);

        if (usuario is null)
        {
            hasher.Verificar(dados.Senha, hash: null);
            throw new CredenciaisInvalidasException();
        }

        if (usuario.EstaBloqueado(agora))
            throw new MuitasTentativasException();

        if (!hasher.Verificar(dados.Senha, usuario.SenhaHash))
        {
            usuario.RegistrarFalhaDeLogin(agora);
            await unitOfWork.SalvarAsync(ct);
            throw usuario.EstaBloqueado(agora) ? new MuitasTentativasException() : new CredenciaisInvalidasException();
        }

        if (!usuario.Ativo)
            throw new CredenciaisInvalidasException();

        usuario.RegistrarLoginBemSucedido(agora);
        await unitOfWork.SalvarAsync(ct);
        return tokens.Gerar(usuario);
    }

    private async Task<Domain.Usuarios.Usuario?> BuscarUsuarioAsync(string email, CancellationToken ct)
    {
        try
        {
            return await usuarios.ObterPorEmailAsync(Email.Criar(email), ct);
        }
        catch (ValidacaoException)
        {
            return null; // e-mail mal formatado é tratado como inexistente
        }
    }
}
