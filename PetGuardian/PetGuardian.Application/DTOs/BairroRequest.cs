using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record BairroRequest(
    [Required][StringLength(30, MinimumLength = 2)] string NomeBairro,
    [Required] Guid CidadeId)
{
    public Bairro ToDomain() => new(NomeBairro, CidadeId);
}