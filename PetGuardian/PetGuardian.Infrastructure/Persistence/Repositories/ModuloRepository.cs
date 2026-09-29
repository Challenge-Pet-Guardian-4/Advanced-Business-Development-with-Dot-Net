using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class ModuloRepository(PetGuardianContext context)
    : Repository<Modulo>(context), IModuloRepository
{
    public IReadOnlyList<Modulo> GetByTrilhaId(Guid trilhaId) =>
        Context.Modulos.AsNoTracking()
            .Where(m => m.TrilhaId == trilhaId).OrderBy(m => m.Nome).ToList();
}