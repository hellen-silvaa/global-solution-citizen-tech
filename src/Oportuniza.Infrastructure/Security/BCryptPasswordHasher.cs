using Oportuniza.Application.Ports.Services;

namespace Oportuniza.Infrastructure.Security;

/// <summary>Hash de senha com BCrypt (salt aleatório embutido no hash, custo 11).</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int Custo = 11;

    // Hash de uma senha aleatória, usado só para gastar o mesmo tempo quando o usuário não existe.
    private static readonly string HashFicticio = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), Custo);

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, Custo);

    public bool Verificar(string senha, string? hash)
    {
        if (hash is null)
        {
            BCrypt.Net.BCrypt.Verify(senha, HashFicticio);
            return false;
        }
        return BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
