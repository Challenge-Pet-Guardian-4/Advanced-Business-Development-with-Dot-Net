using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Corpo do PUT de usuário. Permite atualizar opcionalmente telefone e endereço.</summary>
public record UsuarioUpdateRequest(
    [Required][StringLength(100, MinimumLength = 2)] string Nome,
    [Required][EmailAddress][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6)] string Senha,
    [Required] RoleUsuario Role,
    [StringLength(2, MinimumLength = 2)] string? Ddd = null,
    [StringLength(9, MinimumLength = 8)] string? NumeroTelefone = null,
    EnderecoRequest? Endereco = null
);