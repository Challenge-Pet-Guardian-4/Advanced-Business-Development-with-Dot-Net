using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Corpo do PUT de usuário. Permite atualizar opcionalmente telefone e endereço.</summary>
public record UsuarioUpdateRequest(
    [Required][StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")] string Nome,
    [Required][EmailAddress(ErrorMessage = "E-mail com formato inválido.")][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6, ErrorMessage = "A senha deve conter no mínimo 6 caracteres.")] string Senha,
    [Required] RoleUsuario Role,
    [RegularExpression(@"^\d{2}$", ErrorMessage = "O DDD deve conter exatamente 2 dígitos numéricos.")] string? Ddd = null,
    [RegularExpression(@"^\d{8,9}$", ErrorMessage = "O número deve conter entre 8 e 9 dígitos numéricos.")] string? NumeroTelefone = null,
    EnderecoRequest? Endereco = null
);
