using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface ITrilhaService
{
    PagedResponse<TrilhaResponse> Search(PageQuery query, TrilhaFilter filter);
    TrilhaResponse? GetById(Guid id);
    IReadOnlyList<TrilhaResponse> GetByPetId(Guid petId);
    TrilhaResponse Create(TrilhaRequest request);
    TrilhaResponse? Update(Guid id, TrilhaUpdateRequest request);
    bool Delete(Guid id);
}
