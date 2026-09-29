using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record UsuarioPetResponse(Guid UsuarioId, Guid PetId, bool ResponPrinc)
{
    public static UsuarioPetResponse FromDomain(UsuarioPet up) => new(up.UsuarioId, up.PetId, up.ResponPrinc);
}