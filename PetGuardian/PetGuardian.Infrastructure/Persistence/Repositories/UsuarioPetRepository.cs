using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

/// <summary>Adicionado Update (persiste a alternância de responsável principal feita via PUT).</summary>
public sealed class UsuarioPetRepository(PetGuardianContext context) : IUsuarioPetRepository
{
    public IReadOnlyList<UsuarioPet> GetAll() =>
        context.UsuarioPets.AsNoTracking().ToList();

    public IReadOnlyList<UsuarioPet> GetByUsuarioId(Guid usuarioId) =>
        context.UsuarioPets.AsNoTracking().Where(up => up.UsuarioId == usuarioId).ToList();

    public IReadOnlyList<UsuarioPet> GetByPetId(Guid petId) =>
        context.UsuarioPets.AsNoTracking().Where(up => up.PetId == petId).ToList();

    public IReadOnlyList<UsuarioPet> GetByPetIds(IEnumerable<Guid> petIds) =>
        context.UsuarioPets.AsNoTracking().Where(up => petIds.Contains(up.PetId)).ToList();

    public UsuarioPet? GetByUsuarioAndPet(Guid usuarioId, Guid petId) =>
        context.UsuarioPets.FirstOrDefault(up => up.UsuarioId == usuarioId && up.PetId == petId);

    public UsuarioPet Add(UsuarioPet entity)
    {
        context.UsuarioPets.Add(entity);
        context.SaveChanges();
        return entity;
    }
    
    public UsuarioPet Update(UsuarioPet entity)
    {
        context.UsuarioPets.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(Guid usuarioId, Guid petId)
    {
        var entity = GetByUsuarioAndPet(usuarioId, petId);
        if (entity is null) return false;
        context.UsuarioPets.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public bool Exists(Guid usuarioId, Guid petId) =>
        context.UsuarioPets.Any(up => up.UsuarioId == usuarioId && up.PetId == petId);
}