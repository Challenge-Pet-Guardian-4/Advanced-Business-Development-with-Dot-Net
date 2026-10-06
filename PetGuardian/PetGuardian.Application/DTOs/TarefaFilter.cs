using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.DTOs;

public sealed class TarefaFilter
{
    [StringLength(30)] public string? Titulo { get; set; }
    public Guid? PetId { get; set; }
    public Guid? UsuarioId { get; set; }
    public Guid? StatusId { get; set; }
    /// <summary>true = só concluídas; false = só em aberto.</summary>
    public bool? Concluida { get; set; }
    public DateTime? PrazoDe { get; set; }
    public DateTime? PrazoAte { get; set; }
}