using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IModuloService
{
    IReadOnlyList<ModuloResponse> GetAll();
    ModuloResponse? GetById(Guid id);
    IReadOnlyList<ModuloResponse> GetByTrilhaId(Guid trilhaId);
    ModuloResponse Create(ModuloRequest request);
    ModuloResponse? Update(Guid id, ModuloUpdateRequest request);
    bool Delete(Guid id);
}