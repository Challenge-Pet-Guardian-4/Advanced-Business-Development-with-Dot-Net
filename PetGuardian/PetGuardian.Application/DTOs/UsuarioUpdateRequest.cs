using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Corpo do PUT de usuário. TelefoneId não é reatribuível por aqui.</summary>
public record UsuarioUpdateRequest(
    [Required][StringLength(100, MinimumLength = 2)] string Nome,
    [Required][EmailAddress][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6)] string Senha,
    [Required] RoleUsuario Role
);