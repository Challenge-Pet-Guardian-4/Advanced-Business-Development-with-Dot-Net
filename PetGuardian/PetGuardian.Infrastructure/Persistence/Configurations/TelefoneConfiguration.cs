using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class TelefoneConfiguration : IEntityTypeConfiguration<Telefone>
{
    public void Configure(EntityTypeBuilder<Telefone> builder)
    {
        builder.ToTable("telefone");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id_telefone");

        builder.Property(t => t.NumDdd)
            .HasColumnName("num_ddd")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.NumTel)
            .HasColumnName("num_tel")
            .HasMaxLength(9)
            .IsRequired();
    }
}
