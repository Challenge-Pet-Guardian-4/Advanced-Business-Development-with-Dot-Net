using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface IHistoricoRepository : IRepository<Historico>
{
    IReadOnlyList<Historico> GetByPetId(Guid petId);
}