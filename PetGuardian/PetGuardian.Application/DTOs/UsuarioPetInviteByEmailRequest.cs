using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public record UsuarioPetInviteByEmailRequest(
    [Required] Guid AdminUsuarioId,
    [Required][EmailAddress] string Email,
    [Required] Guid PetId
);
