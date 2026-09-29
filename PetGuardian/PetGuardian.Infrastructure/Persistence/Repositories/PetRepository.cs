using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class PetRepository(PetGuardianContext context)
    : Repository<Pet>(context), IPetRepository
{
    public IReadOnlyList<Pet> GetByRacaId(Guid racaId) =>
        Context.Pets.AsNoTracking().Where(p => p.RacaId == racaId).OrderBy(p => p.Nome).ToList();
}