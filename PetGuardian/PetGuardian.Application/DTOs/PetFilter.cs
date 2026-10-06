using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

public sealed class PetFilter
{
    [StringLength(30)] public string? Nome { get; set; }
    public Guid? RacaId { get; set; }
    public PortePet? Porte { get; set; }
    public SexoPet? Sexo { get; set; }
    public bool? Castrado { get; set; }
}