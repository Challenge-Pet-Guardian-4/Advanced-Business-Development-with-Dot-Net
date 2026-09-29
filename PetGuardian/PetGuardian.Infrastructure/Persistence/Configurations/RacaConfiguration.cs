using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class RacaConfiguration : IEntityTypeConfiguration<Raca>
{
    public void Configure(EntityTypeBuilder<Raca> builder)
    {
        builder.ToTable("raca");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id_raca");

        builder.Property(r => r.NomeRaca)
            .HasColumnName("nome_raca")
            .HasMaxLength(30)
            .IsRequired();
    }
}
