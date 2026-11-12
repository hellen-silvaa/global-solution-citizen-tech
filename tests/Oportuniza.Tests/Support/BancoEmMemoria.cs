using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Oportuniza.Infrastructure.Persistence;

namespace Oportuniza.Tests.Support;

/// <summary>
/// Banco SQLite em memória com o mesmo modelo do EF Core, para testar repositórios e consultas
/// LINQ de verdade (SQL gerado e executado), sem precisar de MySQL.
/// </summary>
public sealed class BancoEmMemoria : IDisposable
{
    private readonly SqliteConnection _conexao = new("DataSource=:memory:");

    public BancoEmMemoria()
    {
        _conexao.Open();
        using var db = NovoContexto();
        db.Database.EnsureCreated();
    }

    /// <summary>Cada chamada devolve um contexto novo (sem cache), como em requisições diferentes.</summary>
    public OportunizaDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<OportunizaDbContext>().UseSqlite(_conexao).Options);

    public void Dispose() => _conexao.Dispose();
}
