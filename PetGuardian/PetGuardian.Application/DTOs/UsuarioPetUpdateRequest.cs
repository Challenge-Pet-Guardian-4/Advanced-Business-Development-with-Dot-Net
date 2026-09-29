using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

/// <summary>Corpo do PUT do vínculo usuário-pet — só permite alternar o responsável principal.</summary>
public record UsuarioPetUpdateRequest(
    [Required] bool ResponPrinc
);