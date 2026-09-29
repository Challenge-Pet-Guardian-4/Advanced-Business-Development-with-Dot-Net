using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Ganhou Role (COMUM/PREMIUM); senha aceita até 60 caracteres.</summary>
public record UsuarioRequest(
    [Required][StringLength(100, MinimumLength = 2)] string Nome,
    [Required][EmailAddress][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6)] string Senha,
    [Required] RoleUsuario Role,
    [Required] Guid TelefoneId)
{
    public Usuario ToDomain() => new(Nome, Email, Senha, Role, TelefoneId);
}