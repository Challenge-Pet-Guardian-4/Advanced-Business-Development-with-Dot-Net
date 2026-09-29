using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class HistoricoConfiguration : IEntityTypeConfiguration<Historico>
{
    public void Configure(EntityTypeBuilder<Historico> builder)
    {
        builder.ToTable("historico");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id_hist");
        builder.Property(h => h.TipoHist).HasColumnName("tipo_hist").HasMaxLength(30).IsRequired();
        builder.Property(h => h.DataHist).HasColumnName("data_hist").IsRequired();
        builder.Property(h => h.PetId).HasColumnName("pet_id_pet").IsRequired();

        builder.HasOne(h => h.Pet)
            .WithMany(p => p.Historicos)
            .HasForeignKey(h => h.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}