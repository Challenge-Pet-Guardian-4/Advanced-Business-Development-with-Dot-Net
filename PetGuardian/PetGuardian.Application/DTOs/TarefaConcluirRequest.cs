using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public record TarefaConcluirRequest(
    [Required] Guid UsuarioId
);
