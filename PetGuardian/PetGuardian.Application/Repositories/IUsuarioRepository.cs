using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

/// <summary>
/// Contrato de persistência para <see cref="Usuario"/>.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Busca usuário pelo e-mail (case-insensitive).</summary>
    Usuario? GetByEmail(string email);

    /// <summary>Verifica existência pelo e-mail.</summary>
    bool ExistsByEmail(string email);
}