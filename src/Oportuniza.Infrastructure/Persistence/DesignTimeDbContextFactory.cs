using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oportuniza.Infrastructure.Persistence;

/// <summary>
/// Usada só pelas ferramentas do EF Core (dotnet ef migrations / database update).
/// Lê a connection string da variável de ambiente ConnectionStrings__Default; sem ela, aponta para o MySQL
/// local de desenvolvimento SEM senha: defina a variável para informar a senha. Nenhum segredo fica no código.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OportunizaDbContext>
{
    private const string ConexaoDesenvolvimento = "Server=localhost;Port=3306;Database=oportuniza;User=root;";

    public OportunizaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default") is { Length: > 0 } valor
            ? valor
            : ConexaoDesenvolvimento;
        var options = new DbContextOptionsBuilder<OportunizaDbContext>().UseMySQL(connectionString).Options;
        return new OportunizaDbContext(options);
    }
}
