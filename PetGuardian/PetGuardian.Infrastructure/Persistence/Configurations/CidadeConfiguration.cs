using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class CidadeConfiguration : IEntityTypeConfiguration<Cidade>
{
    public void Configure(EntityTypeBuilder<Cidade> builder)
    {
        builder.ToTable("cidade");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id_cidade");

        builder.Property(c => c.NomeCidade)
            .HasColumnName("nome_cidade")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.EstadoId)
            .HasColumnName("estado_id_estado")
            .IsRequired();

        builder.HasOne(c => c.Estado)
            .WithMany(e => e.Cidades)
            .HasForeignKey(c => c.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
