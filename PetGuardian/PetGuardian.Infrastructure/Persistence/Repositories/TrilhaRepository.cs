using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class TrilhaRepository(PetGuardianContext context)
    : Repository<Trilha>(context), ITrilhaRepository
{
    public IReadOnlyList<Trilha> GetByPetId(Guid petId) =>
        Context.Trilhas.AsNoTracking()
            .Where(t => t.PetId == petId).OrderBy(t => t.Nome).ToList();
}