using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface IPetRepository : IRepository<Pet>
{
    IReadOnlyList<Pet> GetByRacaId(Guid racaId);
}