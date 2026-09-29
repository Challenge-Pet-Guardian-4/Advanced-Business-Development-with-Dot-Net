using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

/// <summary>
/// Credenciais para requisição de autenticação / login.
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail com formato inválido.")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve conter no mínimo 6 caracteres.")]
    string Senha
);
