using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Api.Infrastructure;
using Oportuniza.Application;
using Oportuniza.Infrastructure;
using Oportuniza.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Servidor: sem cabeçalho "Server" e com limite de tamanho do corpo (1 MB) contra abuso.
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.AddServerHeader = false;
    kestrel.Limits.MaxRequestBodySize = 1024 * 1024;
});

// Camadas (composition root: o único lugar que conhece todas as camadas)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Web API. O encoder escapa < > & ' " (defesa contra XSS caso algum front exiba o texto como HTML),
// mas mantém acentos legíveis.
var encoderSeguro = JavaScriptEncoder.Create(UnicodeRanges.All);
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Encoder = encoderSeguro;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    })
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ValidacaoResponse.Criar);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Encoder = encoderSeguro);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Segurança
builder.Services.AddAutenticacaoJwt();
builder.Services.AddLimitesDeRequisicao(builder.Configuration);
builder.Services.AddCors(o => o.AddDefaultPolicy(policy =>
{
    var origens = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
    policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Location");
}));
builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(365));

// Documentação automática (Swagger / OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AplicarMigracoesAoIniciar"))
    await app.Services.AplicarMigracoesAsync();

app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (app.Configuration.GetValue<bool>("Seguranca:ForcarHttps"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
if (app.Configuration.GetValue("Swagger:Habilitado", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Oportuniza API v1");
        o.DocumentTitle = "Oportuniza API";
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", HealthCheckResponse.Opcoes()).DisableRateLimiting();

await app.RunAsync();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
