using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IHistoricoService
{
    IReadOnlyList<HistoricoResponse> GetAll();
    HistoricoResponse? GetById(Guid id);
    IReadOnlyList<HistoricoResponse> GetByPetId(Guid petId);
    HistoricoResponse Create(HistoricoRequest request);
    HistoricoResponse? Update(Guid id, HistoricoUpdateRequest request);
    bool Delete(Guid id);
}