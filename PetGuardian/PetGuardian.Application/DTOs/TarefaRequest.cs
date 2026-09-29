using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>"VeterinarioId" foi removido; "UsuarioId" agora é obrigatório na criação.</summary>
public record TarefaRequest(
    [Required][StringLength(30, MinimumLength = 2)] string Titulo,
    [Range(0, 999)] int PontosTarefa,
    [Required][StringLength(200, MinimumLength = 2)] string Descricao,
    [Required] DateTime Prazo,
    [Required] Guid PetId,
    [Required] Guid UsuarioId)
{
    public Tarefa ToDomain(Guid statusId) =>
        new(Titulo, PontosTarefa, Descricao, Prazo, PetId, statusId, UsuarioId);
}

/// <summary>Corpo do PUT. Pet e Usuario da tarefa não são reatribuíveis por aqui.</summary>
public record TarefaUpdateRequest(
    [Required][StringLength(30, MinimumLength = 2)] string Titulo,
    [Range(0, 999)] int PontosTarefa,
    [Required][StringLength(200, MinimumLength = 2)] string Descricao,
    [Required] DateTime Prazo
);