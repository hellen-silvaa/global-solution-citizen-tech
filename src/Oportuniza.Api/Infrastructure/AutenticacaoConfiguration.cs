using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oportuniza.Infrastructure.Security;

namespace Oportuniza.Api.Infrastructure;

public static class AutenticacaoConfiguration
{
    /// <summary>
    /// Autenticação JWT endurecida: só aceita tokens assinados com HS256 pelo nosso segredo,
    /// com emissor, audiência e expiração obrigatórios. Respostas 401/403 no formato {"mensagem"}.
    /// </summary>
    public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt.Value.ChaveDeAssinatura(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256], // bloqueia "alg: none" e troca de algoritmo
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new DadosErro("Token ausente, inválido ou expirado!"));
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return context.Response.WriteAsJsonAsync(new DadosErro("Acesso negado!"));
                    }
                };
            });
        services.AddAuthorization();
        return services;
    }
}
