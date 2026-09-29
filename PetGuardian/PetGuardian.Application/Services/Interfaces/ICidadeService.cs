using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface ICidadeService
{
    IReadOnlyList<CidadeResponse> GetAll();
    CidadeResponse? GetById(Guid id);
    IReadOnlyList<CidadeResponse> GetByEstadoId(Guid estadoId);
    CidadeResponse Create(CidadeRequest request);
    CidadeResponse? Update(Guid id, CidadeRequest request);
    bool Delete(Guid id);
}