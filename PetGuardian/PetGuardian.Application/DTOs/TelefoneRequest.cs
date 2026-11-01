using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TelefoneRequest(
    [Required]
    [RegularExpression(@"^\d{2}$", ErrorMessage = "O DDD deve conter exatamente 2 dígitos numéricos.")]
    string NumDdd,
    [Required]
    [RegularExpression(@"^\d{8,9}$", ErrorMessage = "O número deve conter entre 8 e 9 dígitos numéricos.")]
    string NumTel)
{
    public Telefone ToDomain() => new(NumDdd, NumTel);
}