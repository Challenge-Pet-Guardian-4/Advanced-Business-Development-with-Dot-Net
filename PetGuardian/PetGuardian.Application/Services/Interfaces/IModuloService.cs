using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IModuloService
{
    PagedResponse<ModuloResponse> Search(PageQuery query, ModuloFilter filter);
    ModuloResponse? GetById(Guid id);
    IReadOnlyList<ModuloResponse> GetByTrilhaId(Guid trilhaId);
    ModuloResponse Create(ModuloRequest request);
    ModuloResponse? Update(Guid id, ModuloUpdateRequest request);
    bool Delete(Guid id);
}
