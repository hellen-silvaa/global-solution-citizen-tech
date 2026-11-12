using Microsoft.OpenApi;

namespace Oportuniza.Api.Infrastructure;

public static class SwaggerConfiguration
{
    private const string Esquema = "Bearer";

    /// <summary>Swagger com descrição da API, comentários XML dos controllers e botão Authorize (JWT).</summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Oportuniza API",
                Version = "v1",
                Description = "CitizenTech da Global Solution FIAP 2026: rede que conecta cidadãos que buscam " +
                              "qualificação a cursos, oficinas, mentorias e vagas de jovem aprendiz gratuitas, " +
                              "com participação cidadã (propostas e votos por bairro).\n\n" +
                              "**Como autenticar:** faça `POST /auth/login`, copie o `token` e clique em **Authorize**."
            });

            c.AddSecurityDefinition(Esquema, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Cole apenas o token (sem o prefixo Bearer)."
            });
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(Esquema, document)] = []
            });

            c.SchemaFilter<ExemplosSchemaFilter>();
            c.OperationFilter<DescricaoParametrosOperationFilter>();

            // Comentários XML da API (controllers) e da Application (DTOs: descrição de cada campo).
            foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "Oportuniza.*.xml"))
                c.IncludeXmlComments(xml);
        });
        return services;
    }
}
