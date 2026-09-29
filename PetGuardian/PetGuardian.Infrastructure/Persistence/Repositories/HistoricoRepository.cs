using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class HistoricoRepository(PetGuardianContext context)
    : Repository<Historico>(context), IHistoricoRepository
{
    public IReadOnlyList<Historico> GetByPetId(Guid petId) =>
        Context.Historicos.AsNoTracking()
            .Where(h => h.PetId == petId).OrderByDescending(h => h.DataHist).ToList();
}