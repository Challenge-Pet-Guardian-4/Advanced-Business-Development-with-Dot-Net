using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

/// <summary>GetByVeterinarioId removido.</summary>
public sealed class TarefaRepository(PetGuardianContext context)
    : Repository<Tarefa>(context), ITarefaRepository
{
    public IReadOnlyList<Tarefa> GetByPetId(Guid petId) =>
        Context.Tarefas.AsNoTracking()
            .Where(t => t.PetId == petId).OrderBy(t => t.Prazo).ToList();

    public IReadOnlyList<Tarefa> GetByUsuarioId(Guid usuarioId) =>
        Context.Tarefas.AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Prazo).ToList();

    public IReadOnlyList<Tarefa> GetByStatusId(Guid statusId) =>
        Context.Tarefas.AsNoTracking()
            .Where(t => t.StatusId == statusId).OrderBy(t => t.Prazo).ToList();
}