using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class TrilhaFilter
{
    [StringLength(50)] public string? Nome { get; set; }
    public Guid? PetId { get; set; }
}
