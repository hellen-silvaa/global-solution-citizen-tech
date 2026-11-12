using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Usuarios;
using Oportuniza.Domain.ValueObjects;

namespace Oportuniza.Infrastructure.Persistence.Configurations;

/// <summary>
/// Hierarquia de usuários em uma única tabela (TPH). A coluna "tipo" diz se a linha é
/// ADMIN, CIDADAO ou ORGANIZACAO; colunas específicas de cada subtipo ficam nulas nos demais.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.HasDiscriminator<string>("tipo")
            .HasValue<Administrador>("ADMIN")
            .HasValue<Cidadao>("CIDADAO")
            .HasValue<Organizacao>("ORGANIZACAO");
        builder.Property<string>("tipo").HasMaxLength(20);

        builder.Property(u => u.Nome).HasColumnName("nome").HasMaxLength(120).IsRequired();

        // Value object Email gravado como texto; ao ler do banco, é revalidado pelo Email.Criar.
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(Email.TamanhoMaximo)
            .HasConversion(email => email.Valor, valor => Email.Criar(valor))
            .IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.SenhaHash).HasColumnName("senha_hash").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Ativo).HasColumnName("ativo");
        builder.Property(u => u.CriadoEm).HasColumnName("criado_em");
        builder.Property(u => u.TentativasLoginFalhas).HasColumnName("tentativas_login_falhas");
        builder.Property(u => u.BloqueadoAte).HasColumnName("bloqueado_ate");
        builder.Property(u => u.UltimoLoginEm).HasColumnName("ultimo_login_em");
        builder.Ignore(u => u.Perfil);

        builder.OwnsOne(u => u.Endereco, endereco =>
        {
            endereco.Property(e => e.Cep).HasColumnName("endereco_cep").HasMaxLength(8);
            endereco.Property(e => e.Logradouro).HasColumnName("endereco_logradouro").HasMaxLength(150);
            endereco.Property(e => e.Numero).HasColumnName("endereco_numero").HasMaxLength(10);
            endereco.Property(e => e.Bairro).HasColumnName("endereco_bairro").HasMaxLength(80);
            endereco.Property(e => e.Cidade).HasColumnName("endereco_cidade").HasMaxLength(80);
            endereco.Property(e => e.Uf).HasColumnName("endereco_uf").HasMaxLength(2);
        });
    }
}

public class CidadaoConfiguration : IEntityTypeConfiguration<Cidadao>
{
    public void Configure(EntityTypeBuilder<Cidadao> builder)
    {
        builder.Property(c => c.DataNascimento).HasColumnName("data_nascimento");
        builder.Property(c => c.Escolaridade).HasColumnName("escolaridade");

        // Lista de áreas de interesse gravada como texto separado por vírgula ("Tecnologia,Idiomas").
        builder.Ignore(c => c.AreasInteresse);
        builder.Property<List<AreaConhecimento>>("_areasInteresse")
            .HasColumnName("areas_interesse")
            .HasMaxLength(300)
            .HasConversion(new ValueConverter<List<AreaConhecimento>, string>(
                lista => string.Join(',', lista),
                texto => texto.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Enum.Parse<AreaConhecimento>)
                    .ToList()))
            .Metadata.SetValueComparer(new ValueComparer<List<AreaConhecimento>>(
                (a, b) => a!.SequenceEqual(b!),
                lista => lista.Aggregate(0, (hash, area) => HashCode.Combine(hash, area)),
                lista => lista.ToList()));
    }
}

public class OrganizacaoConfiguration : IEntityTypeConfiguration<Organizacao>
{
    public void Configure(EntityTypeBuilder<Organizacao> builder)
    {
        builder.Property(o => o.TipoOrganizacao).HasColumnName("tipo_organizacao");
        builder.Property(o => o.Descricao).HasColumnName("descricao").HasMaxLength(500);
        builder.Property(o => o.Site).HasColumnName("site").HasMaxLength(200);
    }
}
