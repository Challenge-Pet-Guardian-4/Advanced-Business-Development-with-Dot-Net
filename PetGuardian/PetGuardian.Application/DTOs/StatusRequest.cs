using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>Valores aceitos: CONCLUIDO | EXPIRADO | PENDENTE</summary>
public record StatusRequest(
    [Required][StringLength(15, MinimumLength = 2)] string NomeStatus)
{
    public Status ToDomain() => new(NomeStatus);
}