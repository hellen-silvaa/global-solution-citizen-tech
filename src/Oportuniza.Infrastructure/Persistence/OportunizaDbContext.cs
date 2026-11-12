using Microsoft.EntityFrameworkCore;
using Oportuniza.Application.Ports.Persistence;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Oportunidades;
using Oportuniza.Domain.Propostas;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Infrastructure.Persistence.Configurations;

namespace Oportuniza.Infrastructure.Persistence;

/// <summary>
/// Contexto do EF Core. Também é a Unit of Work: os serviços chamam <see cref="SalvarAsync"/>
/// uma vez por caso de uso, o que garante que tudo é gravado na mesma transação.
/// </summary>
public class OportunizaDbContext(DbContextOptions<OportunizaDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cidadao> Cidadaos => Set<Cidadao>();
    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Oportunidade> Oportunidades => Set<Oportunidade>();
    public DbSet<Inscricao> Inscricoes => Set<Inscricao>();
    public DbSet<Proposta> Propostas => Set<Proposta>();
    public DbSet<Voto> Votos => Set<Voto>();

    public Task<int> SalvarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new CidadaoConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizacaoConfiguration());
        modelBuilder.ApplyConfiguration(new OportunidadeConfiguration());
        modelBuilder.ApplyConfiguration(new InscricaoConfiguration());
        modelBuilder.ApplyConfiguration(new PropostaConfiguration());
        modelBuilder.ApplyConfiguration(new VotoConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enums gravados como texto (legível no banco e estável se a ordem do enum mudar).
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(30);
    }
}
