using Microsoft.EntityFrameworkCore;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence;

/// <summary>
/// DbSets de Atendimento/Clinica/TipoAtend/Veterinario removidos (fora do banco de dados atual).
/// Adicionados Trilhas/Modulos/Aulas/Historicos.
/// </summary>
public class PetGuardianContext(DbContextOptions<PetGuardianContext> options) : DbContext(options)
{
    // Hierarquia de endereço
    public DbSet<Estado>   Estados   { get; set; }
    public DbSet<Cidade>   Cidades   { get; set; }
    public DbSet<Bairro>   Bairros   { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }

    // Lookup
    public DbSet<Raca>     Racas     { get; set; }
    public DbSet<Status>   Status    { get; set; }
    public DbSet<Telefone> Telefones { get; set; }

    // Core
    public DbSet<Usuario>   Usuarios   { get; set; }
    public DbSet<Pet>       Pets       { get; set; }
    public DbSet<Tarefa>    Tarefas    { get; set; }
    public DbSet<Trilha>    Trilhas    { get; set; }
    public DbSet<Modulo>    Modulos    { get; set; }
    public DbSet<Aula>      Aulas      { get; set; }
    public DbSet<Historico> Historicos { get; set; }

    // Join tables
    public DbSet<UsuarioPet>      UsuarioPets      { get; set; }
    public DbSet<UsuarioEndereco> UsuarioEnderecos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entidades sem Id convencional — chaves declaradas antes do scan
        modelBuilder.Entity<UsuarioPet>()
            .HasKey(up => new { up.UsuarioId, up.PetId });

        modelBuilder.Entity<UsuarioEndereco>()
            .HasKey(ue => new { ue.UsuarioId, ue.EnderecoId });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetGuardianContext).Assembly);

        // Padronização Oracle: tabelas e colunas em UPPERCASE evitam erro ORA-00942 de case sensitivity com aspas duplas
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entity.SetTableName(tableName.ToUpperInvariant());

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                    property.SetColumnName(columnName.ToUpperInvariant());
            }
        }
    }
}