using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

/// <summary>"respon_princ" passou de CHAR(1) para NUMBER.</summary>
public sealed class UsuarioPetConfiguration : IEntityTypeConfiguration<UsuarioPet>
{
    public void Configure(EntityTypeBuilder<UsuarioPet> builder)
    {
        builder.ToTable("usuario_pet");
        builder.HasKey(up => new { up.UsuarioId, up.PetId });
        builder.Property(up => up.UsuarioId).HasColumnName("usuario_id_usuario").IsRequired();
        builder.Property(up => up.PetId).HasColumnName("pet_id_pet").IsRequired();

        // respon_princ NUMBER (Antes era CHAR(1))
        builder.Property(up => up.ResponPrinc)
            .HasColumnName("respon_princ")
            .HasColumnType("NUMBER(1)")
            .HasConversion(
                v => v ? 1 : 0,
                v => v == 1)
            .IsRequired();

        builder.HasOne(up => up.Usuario)
            .WithMany(u => u.PetsVinculados)
            .HasForeignKey(up => up.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Pet)
            .WithMany(p => p.UsuariosPet)
            .HasForeignKey(up => up.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}