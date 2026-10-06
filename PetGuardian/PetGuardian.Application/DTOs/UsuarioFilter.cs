using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

public sealed class UsuarioFilter
{
    [StringLength(100)] public string? Nome { get; set; }
    [StringLength(50)] public string? Email { get; set; }
    public RoleUsuario? Role { get; set; }
}