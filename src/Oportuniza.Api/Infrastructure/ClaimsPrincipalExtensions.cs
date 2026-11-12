using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Oportuniza.Application.Common;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Exceptions;

namespace Oportuniza.Api.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Converte as claims do token JWT no <see cref="UsuarioAutenticado"/> usado pelos casos de uso.</summary>
    public static UsuarioAutenticado ParaUsuarioAutenticado(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var perfil = principal.FindFirstValue(ClaimTypes.Role);

        if (!long.TryParse(id, out var usuarioId) || !Enum.TryParse<Perfil>(perfil, out var perfilEnum))
            throw new CredenciaisInvalidasException();

        return new UsuarioAutenticado(usuarioId, perfilEnum);
    }
}
