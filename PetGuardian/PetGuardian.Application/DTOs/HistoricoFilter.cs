using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class HistoricoFilter
{
    public Guid? PetId { get; set; }
    [StringLength(30)] public string? TipoHist { get; set; }
    public DateTime? DataDe { get; set; }
    public DateTime? DataAte { get; set; }
}