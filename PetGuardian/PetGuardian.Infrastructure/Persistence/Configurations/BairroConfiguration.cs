using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

public sealed class BairroConfiguration : IEntityTypeConfiguration<Bairro>
{
    public void Configure(EntityTypeBuilder<Bairro> builder)
    {
        builder.ToTable("bairro");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id_bairro");

        builder.Property(b => b.NomeBairro)
            .HasColumnName("nome_bairro")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(b => b.CidadeId)
            .HasColumnName("cidade_id_cidade")
            .IsRequired();

        builder.HasOne(b => b.Cidade)
            .WithMany(c => c.Bairros)
            .HasForeignKey(b => b.CidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
