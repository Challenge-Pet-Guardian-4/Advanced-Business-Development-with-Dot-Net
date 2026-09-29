using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface ITrilhaRepository : IRepository<Trilha>
{
    IReadOnlyList<Trilha> GetByPetId(Guid petId);
}