using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record HistoricoRequest(
    [Required][StringLength(30, MinimumLength = 2)] string TipoHist,
    [Required] DateTime DataHist,
    [Required] Guid PetId)
{
    public Historico ToDomain() => new(TipoHist, DataHist, PetId);
}

/// <summary>Corpo do PUT. O pet vinculado não é reatribuível por aqui.</summary>
public record HistoricoUpdateRequest(
    [Required][StringLength(30, MinimumLength = 2)] string TipoHist,
    [Required] DateTime DataHist
);