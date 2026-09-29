using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>"Idade" (int) foi substituído por "DataNascimento" (DATE), acompanhando as novas mudanças do banco de dados.</summary>
public record PetRequest(
    [Required][StringLength(30, MinimumLength = 2)] string Nome,
    [Required] DateTime DataNascimento,
    [Required] SexoPet Sexo,
    [Required] PortePet Porte,
    bool Castrado,
    [Required] Guid RacaId)
{
    public Pet ToDomain() => new(Nome, DataNascimento, Sexo, Porte, Castrado, RacaId);
}