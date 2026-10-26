using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class AulaFilter
{
    [StringLength(50)] public string? Nome { get; set; }
    public Guid? ModuloId { get; set; }
    [StringLength(20)] public string? Dificuldade { get; set; }
    public bool? Concluida { get; set; }
}
