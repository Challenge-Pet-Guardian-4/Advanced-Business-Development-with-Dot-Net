using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public record EnderecoRequest(
    [Required][StringLength(9, MinimumLength = 8, ErrorMessage = "O CEP deve ter entre 8 e 9 caracteres (ex: 01001-000 ou 01001000)")]
    string Cep,
    [Required][StringLength(5, MinimumLength = 1)]
    string Numero
);