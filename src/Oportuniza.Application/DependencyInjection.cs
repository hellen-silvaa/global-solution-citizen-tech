using Microsoft.Extensions.DependencyInjection;
using Oportuniza.Application.Inscricoes.Regras;
using Oportuniza.Application.Services;
using Oportuniza.Domain.Recomendacoes;

namespace Oportuniza.Application;

public static class DependencyInjection
{
    /// <summary>Registra casos de uso, regras de negócio e serviços de domínio.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        // Casos de uso
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<ICidadaoService, CidadaoService>();
        services.AddScoped<IOrganizacaoService, OrganizacaoService>();
        services.AddScoped<IOportunidadeService, OportunidadeService>();
        services.AddScoped<IInscricaoService, InscricaoService>();
        services.AddScoped<IPropostaService, PropostaService>();
        services.AddScoped<IIndicadorService, IndicadorService>();
        services.AddScoped<IEnderecoService, EnderecoService>();

        // Regras de inscrição que consultam o banco, executadas nesta ordem
        // (duplicidade antes do limite: quem já está inscrito recebe a mensagem mais precisa).
        services.AddScoped<IRegraInscricao, RegraInscricaoUnica>();
        services.AddScoped<IRegraInscricao, RegraLimiteInscricoesAtivas>();

        // Serviço de domínio de recomendação e seus critérios (Strategy)
        services.AddSingleton<ICriterioRecomendacao, CriterioInteresse>();
        services.AddSingleton<ICriterioRecomendacao, CriterioMesmoBairro>();
        services.AddSingleton<ICriterioRecomendacao, CriterioOnline>();
        services.AddSingleton<ICriterioRecomendacao, CriterioGratuita>();
        services.AddSingleton<MotorDeRecomendacao>();

        return services;
    }
}
