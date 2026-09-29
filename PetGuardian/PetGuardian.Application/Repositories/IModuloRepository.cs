using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface IModuloRepository : IRepository<Modulo>
{
    IReadOnlyList<Modulo> GetByTrilhaId(Guid trilhaId);
}