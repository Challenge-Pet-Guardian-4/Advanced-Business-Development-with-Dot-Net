using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IUsuarioRepository"/>.
/// </summary>
public sealed class UsuarioRepository(PetGuardianContext context)
    : Repository<Usuario>(context), IUsuarioRepository
{
    /// <inheritdoc />
    public Usuario? GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizado = email.Trim().ToLowerInvariant();

        return Context.Usuarios
            .AsNoTracking()
            .FirstOrDefault(u => u.Email.ToLower() == normalizado);
    }

    /// <inheritdoc />
    public bool ExistsByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizado = email.Trim().ToLowerInvariant();

        return Context.Usuarios
            .Any(u => u.Email.ToLower() == normalizado);
    }
}