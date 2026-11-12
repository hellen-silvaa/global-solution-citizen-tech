using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Application.Common;

/// <summary>
/// Quem está chamando o caso de uso (extraído do token JWT pela Api).
/// Centraliza as checagens de "é o dono do recurso ou é admin".
/// </summary>
public record UsuarioAutenticado(long Id, Perfil Perfil)
{
    public bool EhAdmin => Perfil == Perfil.Admin;

    public void ExigirPerfil(Perfil perfil)
    {
        if (Perfil != perfil)
            throw new AcessoNegadoException($"Operação permitida apenas para o perfil {perfil}!");
    }

    /// <summary>Permite a operação ao próprio usuário ou a um administrador.</summary>
    public void ExigirProprioOuAdmin(long idDono)
    {
        if (!EhAdmin && Id != idDono)
            throw new AcessoNegadoException();
    }
}
