using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record EstadoRequest(
    [Required][StringLength(30, MinimumLength = 2)] string NomeEstado)
{
    public Estado ToDomain() => new(NomeEstado);
}