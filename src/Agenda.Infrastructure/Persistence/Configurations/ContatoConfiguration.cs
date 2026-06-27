using Agenda.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agenda.Infrastructure.Persistence.Configurations;

internal sealed class ContatoConfiguration : IEntityTypeConfiguration<Contato>
{
    public void Configure(EntityTypeBuilder<Contato> builder)
    {
        builder.ToTable("contatos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20);

        builder.Property(c => c.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(c => c.Excluido)
            .HasColumnName("excluido")
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("ix_contatos_email");

        // Global query filter: hide soft-deleted records by default
        builder.HasQueryFilter(c => !c.Excluido);
    }
}
