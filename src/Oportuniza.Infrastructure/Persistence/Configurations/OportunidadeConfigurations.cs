using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Inscricoes;
using Oportuniza.Domain.Oportunidades;

namespace Oportuniza.Infrastructure.Persistence.Configurations;

public class OportunidadeConfiguration : IEntityTypeConfiguration<Oportunidade>
{
    public void Configure(EntityTypeBuilder<Oportunidade> builder)
    {
        builder.ToTable("oportunidades");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.OrganizacaoId).HasColumnName("organizacao_id");
        builder.HasOne(o => o.Organizacao)
            .WithMany()
            .HasForeignKey(o => o.OrganizacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.Titulo).HasColumnName("titulo").HasMaxLength(120).IsRequired();
        builder.Property(o => o.Descricao).HasColumnName("descricao").HasMaxLength(1000).IsRequired();
        builder.Property(o => o.Tipo).HasColumnName("tipo");
        builder.Property(o => o.Modalidade).HasColumnName("modalidade");
        builder.Property(o => o.Area).HasColumnName("area");
        builder.Property(o => o.Bairro).HasColumnName("bairro").HasMaxLength(80).IsRequired();
        builder.Property(o => o.CargaHoraria).HasColumnName("carga_horaria");
        builder.Property(o => o.VagasTotais).HasColumnName("vagas_totais");
        builder.Property(o => o.Gratuita).HasColumnName("gratuita");
        builder.Property(o => o.Status).HasColumnName("status");
        builder.Property(o => o.CriadoEm).HasColumnName("criado_em");

        // Value object Periodo nas mesmas colunas de antes (data_inicio / data_fim).
        builder.OwnsOne(o => o.Periodo, periodo =>
        {
            periodo.Property(p => p.Inicio).HasColumnName("data_inicio");
            periodo.Property(p => p.Fim).HasColumnName("data_fim");
        });
        builder.Navigation(o => o.Periodo).IsRequired();

        // Token de concorrência: se duas pessoas pegarem a última vaga ao mesmo tempo,
        // a segunda gravação falha (409) em vez de passar do limite.
        builder.Property(o => o.VagasOcupadas).HasColumnName("vagas_ocupadas").IsConcurrencyToken();

        builder.Ignore(o => o.VagasDisponiveis);
        builder.Ignore(o => o.TemVagas);
        builder.Ignore(o => o.EstaAberta);

        builder.HasIndex(o => new { o.Status, o.Area });
        builder.HasIndex(o => o.Bairro);
    }
}

public class InscricaoConfiguration : IEntityTypeConfiguration<Inscricao>
{
    public void Configure(EntityTypeBuilder<Inscricao> builder)
    {
        builder.ToTable("inscricoes");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.CidadaoId).HasColumnName("cidadao_id");
        builder.HasOne(i => i.Cidadao).WithMany().HasForeignKey(i => i.CidadaoId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.OportunidadeId).HasColumnName("oportunidade_id");
        builder.HasOne(i => i.Oportunidade).WithMany().HasForeignKey(i => i.OportunidadeId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.Status).HasColumnName("status");
        builder.Property(i => i.CriadoEm).HasColumnName("criado_em");
        builder.Property(i => i.AtualizadaEm).HasColumnName("atualizada_em");
        builder.Ignore(i => i.EstaAtiva);

        builder.HasIndex(i => new { i.CidadaoId, i.OportunidadeId });
    }
}
