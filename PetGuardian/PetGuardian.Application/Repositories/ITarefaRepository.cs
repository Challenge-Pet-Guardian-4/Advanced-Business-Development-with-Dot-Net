using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

/// <summary>GetByVeterinarioId removido (Veterinario não existe mais no banco de dados).</summary>
public interface ITarefaRepository : IRepository<Tarefa>
{
    IReadOnlyList<Tarefa> GetByPetId(Guid petId);
    IReadOnlyList<Tarefa> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<Tarefa> GetByStatusId(Guid statusId);
}