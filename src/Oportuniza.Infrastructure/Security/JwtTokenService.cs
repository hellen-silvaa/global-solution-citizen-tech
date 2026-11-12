using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oportuniza.Application.Dtos.Autenticacao;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Domain.Usuarios;

namespace Oportuniza.Infrastructure.Security;

/// <summary>Configuração do JWT (seção "Jwt" do appsettings).</summary>
public class JwtOptions
{
    public const string Secao = "Jwt";
    public const int TamanhoMinimoSegredo = 32;

    /// <summary>Segredo HMAC com no mínimo 32 caracteres. Em produção, vem da variável Jwt__Secret.</summary>
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "Oportuniza.Api";
    public string Audience { get; set; } = "Oportuniza";
    public int ExpiracaoHoras { get; set; } = 2;

    public SymmetricSecurityKey ChaveDeAssinatura() => new(Encoding.UTF8.GetBytes(Secret));
}

public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider relogio) : ITokenService
{
    private readonly JwtOptions _opcoes = options.Value;

    public DadosToken Gerar(Usuario usuario)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var expiraEm = agora.AddHours(_opcoes.ExpiracaoHoras);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email.Valor),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _opcoes.Issuer,
            audience: _opcoes.Audience,
            claims: claims,
            notBefore: agora,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(_opcoes.ChaveDeAssinatura(), SecurityAlgorithms.HmacSha256));

        return new DadosToken(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", expiraEm,
            usuario.Id, usuario.Nome, usuario.Perfil);
    }
}
