using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

/// <summary>
/// Repositório dedicado para a entidade de junção <see cref="UsuarioPet"/> (chave composta).
/// Adicionado Update, necessário para persistir a alternância de responsável principal via PUT.
/// </summary>
public interface IUsuarioPetRepository
{
    IReadOnlyList<UsuarioPet> GetAll();
    IReadOnlyList<UsuarioPet> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<UsuarioPet> GetByPetId(Guid petId);
    IReadOnlyList<UsuarioPet> GetByPetIds(IEnumerable<Guid> petIds);
    UsuarioPet? GetByUsuarioAndPet(Guid usuarioId, Guid petId);
    UsuarioPet Add(UsuarioPet entity);
    UsuarioPet Update(UsuarioPet entity);
    bool Delete(Guid usuarioId, Guid petId);
    bool Exists(Guid usuarioId, Guid petId);
}