using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class AulaRepository(PetGuardianContext context)
    : Repository<Aula>(context), IAulaRepository
{
    public IReadOnlyList<Aula> GetByModuloId(Guid moduloId) =>
        Context.Aulas.AsNoTracking()
            .Where(a => a.ModuloId == moduloId).OrderBy(a => a.Nome).ToList();
}