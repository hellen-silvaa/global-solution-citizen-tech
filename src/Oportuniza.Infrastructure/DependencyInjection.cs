using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Application.Ports.Queries;
using Oportuniza.Application.Ports.Services;
using Oportuniza.Infrastructure.External.ViaCep;
using Oportuniza.Infrastructure.HealthChecks;
using Oportuniza.Infrastructure.Persistence;
using Oportuniza.Infrastructure.Persistence.Queries;
using Oportuniza.Infrastructure.Persistence.Repositories;
using Oportuniza.Infrastructure.Security;

namespace Oportuniza.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra os adaptadores: banco MySQL (EF Core), repositórios, consultas, ViaCEP, JWT, BCrypt e health check.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddDbContext<OportunizaDbContext>(options => options.UseMySQL(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OportunizaDbContext>());

        // Escrita (agregados)
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICidadaoRepository, CidadaoRepository>();
        services.AddScoped<IOrganizacaoRepository, OrganizacaoRepository>();
        services.AddScoped<IOportunidadeRepository, OportunidadeRepository>();
        services.AddScoped<IInscricaoRepository, InscricaoRepository>();
        services.AddScoped<IPropostaRepository, PropostaRepository>();

        // Leitura (consultas agregadas)
        services.AddScoped<IIndicadorQueries, IndicadorQueries>();

        services.AddHttpClient<IConsultaCepGateway, ViaCepClient>(client =>
        {
            var baseUrl = configuration["ViaCep:BaseUrl"] ?? "https://viacep.com.br/ws/";
            client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Secao))
            .Validate(o => o.Secret.Length >= JwtOptions.TamanhoMinimoSegredo,
                $"Jwt:Secret deve ter pelo menos {JwtOptions.TamanhoMinimoSegredo} caracteres (defina a variável de ambiente Jwt__Secret).")
            .ValidateOnStart();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddHealthChecks().AddCheck<BancoDeDadosHealthCheck>("banco");

        return services;
    }
}
