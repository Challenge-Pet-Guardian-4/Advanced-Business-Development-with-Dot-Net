using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("endereco");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id_endereco");

        builder.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8).IsRequired();
        builder.Property(e => e.Rua).HasColumnName("rua").HasMaxLength(150).IsRequired();
        builder.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(5).IsRequired();

        builder.Property(e => e.BairroId).HasColumnName("bairro_id_bairro").IsRequired();
        builder.HasOne(e => e.Bairro)
            .WithMany(b => b.Enderecos)
            .HasForeignKey(e => e.BairroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}