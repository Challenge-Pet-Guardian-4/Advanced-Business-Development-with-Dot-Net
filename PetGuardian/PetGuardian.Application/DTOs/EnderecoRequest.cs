using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public record EnderecoRequest(
    [Required]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve estar no formato 00000-000 ou 00000000 (8 dígitos numéricos).")]
    string Cep,
    [Required][StringLength(10, MinimumLength = 1, ErrorMessage = "O número deve ter entre 1 e 10 caracteres.")]
    string Numero
);