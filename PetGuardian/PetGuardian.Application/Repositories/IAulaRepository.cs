using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface IAulaRepository : IRepository<Aula>
{
    IReadOnlyList<Aula> GetByModuloId(Guid moduloId);
}