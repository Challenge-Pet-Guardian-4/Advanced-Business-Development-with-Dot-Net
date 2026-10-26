using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class ModuloFilter
{
    [StringLength(50)] public string? Nome { get; set; }
    public Guid? TrilhaId { get; set; }
}
