namespace Oportuniza.Application.Ports.Services;

/// <summary>Hash e verificação de senhas (nunca guardamos senha em texto puro).</summary>
public interface IPasswordHasher
{
    string Hash(string senha);

    /// <summary>
    /// Verifica a senha. Com <paramref name="hash"/> nulo (usuário inexistente), faz uma verificação
    /// falsa com o mesmo custo e devolve <c>false</c>: o tempo de resposta não revela se o e-mail existe.
    /// </summary>
    bool Verificar(string senha, string? hash);
}
