using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record ModuloRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(10, MinimumLength = 1)] string TempoConclusao,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao,
    [Required] Guid TrilhaId)
{
    public Modulo ToDomain() => new(Nome, TempoConclusao, Descricao, TrilhaId);
}

/// <summary>Corpo do PUT. A trilha vinculada não é reatribuível por aqui.</summary>
public record ModuloUpdateRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(10, MinimumLength = 1)] string TempoConclusao,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao
);