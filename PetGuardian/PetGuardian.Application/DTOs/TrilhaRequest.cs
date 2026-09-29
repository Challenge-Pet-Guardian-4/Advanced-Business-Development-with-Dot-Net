using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TrilhaRequest(
    [Required][StringLength(30, MinimumLength = 2)] string Nome,
    [Required][StringLength(200, MinimumLength = 2)] string Descricao,
    [Required] Guid PetId)
{
    public Trilha ToDomain() => new(Nome, Descricao, PetId);
}

/// <summary>Corpo do PUT. O pet vinculado não é reatribuível por aqui.</summary>
public record TrilhaUpdateRequest(
    [Required][StringLength(30, MinimumLength = 2)] string Nome,
    [Required][StringLength(200, MinimumLength = 2)] string Descricao
);