using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TelefoneRequest(
    [Required][StringLength(2, MinimumLength = 2, ErrorMessage = "O DDD deve ter 2 dígitos")] string NumDdd,
    [Required][StringLength(9, MinimumLength = 8, ErrorMessage = "O número deve ter entre 8 e 9 dígitos")] string NumTel)
{
    public Telefone ToDomain() => new(NumDdd, NumTel);
}