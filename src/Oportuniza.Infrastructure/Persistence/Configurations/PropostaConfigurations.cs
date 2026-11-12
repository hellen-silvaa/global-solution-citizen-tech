using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Propostas;

namespace Oportuniza.Infrastructure.Persistence.Configurations;

public class PropostaConfiguration : IEntityTypeConfiguration<Proposta>
{
    public void Configure(EntityTypeBuilder<Proposta> builder)
    {
        builder.ToTable("propostas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.AutorId).HasColumnName("autor_id");
        builder.HasOne(p => p.Autor).WithMany().HasForeignKey(p => p.AutorId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.OportunidadeGeradaId).HasColumnName("oportunidade_gerada_id");
        builder.HasOne(p => p.OportunidadeGerada)
            .WithMany()
            .HasForeignKey(p => p.OportunidadeGeradaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.Titulo).HasColumnName("titulo").HasMaxLength(120).IsRequired();
        builder.Property(p => p.Descricao).HasColumnName("descricao").HasMaxLength(1000).IsRequired();
        builder.Property(p => p.Area).HasColumnName("area");
        builder.Property(p => p.Bairro).HasColumnName("bairro").HasMaxLength(80).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status");
        builder.Property(p => p.CriadoEm).HasColumnName("criado_em");

        // Os votos só entram pela Proposta (campo privado _votos).
        builder.HasMany(p => p.Votos)
            .WithOne(v => v.Proposta)
            .HasForeignKey(v => v.PropostaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Votos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(p => p.TotalVotos);
        builder.Ignore(p => p.EstaAberta);

        builder.HasIndex(p => new { p.Status, p.Bairro });
    }
}

public class VotoConfiguration : IEntityTypeConfiguration<Voto>
{
    public void Configure(EntityTypeBuilder<Voto> builder)
    {
        builder.ToTable("votos");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.PropostaId).HasColumnName("proposta_id");
        builder.Property(v => v.CidadaoId).HasColumnName("cidadao_id");
        builder.Property(v => v.CriadoEm).HasColumnName("criado_em");

        builder.HasOne(v => v.Cidadao).WithMany().HasForeignKey(v => v.CidadaoId).OnDelete(DeleteBehavior.Restrict);

        // RN-P1 também garantida no banco: um voto por cidadão por proposta.
        builder.HasIndex(v => new { v.PropostaId, v.CidadaoId }).IsUnique();
    }
}
