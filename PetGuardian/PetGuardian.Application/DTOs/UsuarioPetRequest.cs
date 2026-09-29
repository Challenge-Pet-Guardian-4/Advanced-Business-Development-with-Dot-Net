using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record UsuarioPetRequest(
    [Required] Guid UsuarioId,
    [Required] Guid PetId,
    bool ResponPrinc)
{
    public UsuarioPet ToDomain() => new(UsuarioId, PetId, ResponPrinc);
}