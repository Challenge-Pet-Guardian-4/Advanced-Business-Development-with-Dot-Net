using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable("modulo");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id_modulo");
        builder.Property(m => m.Nome).HasColumnName("nome").HasMaxLength(50).IsRequired();
        builder.Property(m => m.TempoConclusao).HasColumnName("tempo_conclusao").HasMaxLength(10).IsRequired();
        builder.Property(m => m.Descricao).HasColumnName("descricao").HasMaxLength(100).IsRequired();
        builder.Property(m => m.TrilhaId).HasColumnName("trilha_id_trilha").IsRequired();

        builder.HasOne(m => m.Trilha)
            .WithMany(t => t.Modulos)
            .HasForeignKey(m => m.TrilhaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Aulas)
            .WithOne(a => a.Modulo)
            .HasForeignKey(a => a.ModuloId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}