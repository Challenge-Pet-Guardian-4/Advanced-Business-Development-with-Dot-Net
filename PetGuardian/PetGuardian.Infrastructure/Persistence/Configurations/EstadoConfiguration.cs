using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("estado");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id_estado");

        builder.Property(e => e.NomeEstado)
            .HasColumnName("nome_estado")
            .HasMaxLength(30)
            .IsRequired();
    }
}
