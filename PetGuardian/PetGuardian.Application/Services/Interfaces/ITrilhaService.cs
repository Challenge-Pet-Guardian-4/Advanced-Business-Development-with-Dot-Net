using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface ITrilhaService
{
    IReadOnlyList<TrilhaResponse> GetAll();
    TrilhaResponse? GetById(Guid id);
    IReadOnlyList<TrilhaResponse> GetByPetId(Guid petId);
    TrilhaResponse Create(TrilhaRequest request);
    TrilhaResponse? Update(Guid id, TrilhaUpdateRequest request);
    bool Delete(Guid id);
}