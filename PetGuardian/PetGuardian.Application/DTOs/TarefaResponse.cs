using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TarefaResponse(
    Guid      Id,
    string    Titulo,
    int       PontosTarefa,
    string    Descricao,
    DateTime  Criacao,
    DateTime  Prazo,
    DateTime? Conclusao,
    Guid      UsuarioId,
    Guid      PetId,
    Guid      StatusId)
{
    public static TarefaResponse FromDomain(Tarefa t) =>
        new(t.Id, t.Titulo, t.PontosTarefa, t.Descricao,
            t.Criacao, t.Prazo, t.Conclusao,
            t.UsuarioId, t.PetId, t.StatusId);
}