using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public record UsuarioPetInviteByUsuarioRequest(
    [Required] Guid AdminUsuarioId,
    [Required] Guid UsuarioConvidadoId,
    [Required] Guid PetId
);
