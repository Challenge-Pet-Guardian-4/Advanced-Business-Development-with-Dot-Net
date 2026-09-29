using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class TrilhaConfiguration : IEntityTypeConfiguration<Trilha>
{
    public void Configure(EntityTypeBuilder<Trilha> builder)
    {
        builder.ToTable("trilha");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id_trilha");
        builder.Property(t => t.Nome).HasColumnName("nome").HasMaxLength(30).IsRequired();
        builder.Property(t => t.Descricao).HasColumnName("descricao").HasMaxLength(200).IsRequired();
        builder.Property(t => t.PetId).HasColumnName("pet_id_pet").IsRequired();

        builder.HasOne(t => t.Pet)
            .WithMany(p => p.Trilhas)
            .HasForeignKey(t => t.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Modulos)
            .WithOne(m => m.Trilha)
            .HasForeignKey(m => m.TrilhaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}