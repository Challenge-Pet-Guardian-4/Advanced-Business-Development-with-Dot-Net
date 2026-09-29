using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("status");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id_status");
        builder.Property(s => s.NomeStatus).HasColumnName("nome_status").HasMaxLength(15).IsRequired();
    }
}