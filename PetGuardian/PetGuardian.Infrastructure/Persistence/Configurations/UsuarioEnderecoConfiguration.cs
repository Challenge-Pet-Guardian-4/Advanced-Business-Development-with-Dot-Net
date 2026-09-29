using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class UsuarioEnderecoConfiguration : IEntityTypeConfiguration<UsuarioEndereco>
{
    public void Configure(EntityTypeBuilder<UsuarioEndereco> builder)
    {
        builder.ToTable("usuario_endereco");
        builder.HasKey(ue => new { ue.UsuarioId, ue.EnderecoId });

        builder.Property(ue => ue.UsuarioId).HasColumnName("usuario_id_usuario").IsRequired();
        builder.Property(ue => ue.EnderecoId).HasColumnName("endereco_id_endereco").IsRequired();

        builder.HasOne(ue => ue.Usuario)
            .WithMany(u => u.Enderecos)
            .HasForeignKey(ue => ue.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ue => ue.Endereco)
            .WithMany()
            .HasForeignKey(ue => ue.EnderecoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}