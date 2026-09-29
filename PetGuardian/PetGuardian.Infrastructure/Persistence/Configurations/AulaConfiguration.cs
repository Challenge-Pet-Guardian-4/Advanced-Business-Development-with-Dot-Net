using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Configurations;

/// <summary>Adicionadas as colunas "conteudo" (VARCHAR2 1000) e "concluida" (NUMBER).</summary>
public sealed class AulaConfiguration : IEntityTypeConfiguration<Aula>
{
    public void Configure(EntityTypeBuilder<Aula> builder)
    {
        builder.ToTable("aula");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id_aula");
        builder.Property(a => a.Nome).HasColumnName("nome").HasMaxLength(50).IsRequired();
        builder.Property(a => a.Descricao).HasColumnName("descricao").HasMaxLength(100).IsRequired();
        builder.Property(a => a.PontosAula).HasColumnName("pontos_aula").HasColumnType("NUMBER(5)").IsRequired();
        builder.Property(a => a.Dificuldade).HasColumnName("dificuldade").HasMaxLength(20).IsRequired();
        builder.Property(a => a.Conteudo).HasColumnName("conteudo").HasMaxLength(1000).IsRequired();

        // concluida NUMBER (mesmo padrão de conversão bool<->NUMBER(1) usado em Pet.Castrado / UsuarioPet.ResponPrinc)
        builder.Property(a => a.Concluida)
            .HasColumnName("concluida")
            .HasColumnType("NUMBER(1)")
            .HasConversion(
                v => v ? 1 : 0,
                v => v == 1)
            .IsRequired();

        builder.Property(a => a.ModuloId).HasColumnName("modulo_id_modulo").IsRequired();

        builder.HasOne(a => a.Modulo)
            .WithMany(m => m.Aulas)
            .HasForeignKey(a => a.ModuloId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}