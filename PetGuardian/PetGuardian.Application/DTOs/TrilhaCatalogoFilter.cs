using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class TrilhaCatalogoFilter
{
    /// <summary>Busca textual (case-insensitive) em nome e descrição.</summary>
    [StringLength(60)] public string? Termo { get; set; }
    public Guid? PetId { get; set; }
    /// <summary>Dificuldade de ao menos uma aula.</summary>
    [StringLength(20)] public string? Dificuldade { get; set; }
}