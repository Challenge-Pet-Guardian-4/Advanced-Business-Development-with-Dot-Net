using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

public record UsuarioResponse(Guid Id, string Nome, string Email, RoleUsuario Role, Guid TelefoneId)
{
    public static UsuarioResponse FromDomain(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Role, u.TelefoneId);
}