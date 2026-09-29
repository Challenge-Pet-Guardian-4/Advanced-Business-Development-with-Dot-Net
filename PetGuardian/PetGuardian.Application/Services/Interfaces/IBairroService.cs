using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IBairroService
{
    IReadOnlyList<BairroResponse> GetAll();
    BairroResponse? GetById(Guid id);
    IReadOnlyList<BairroResponse> GetByCidadeId(Guid cidadeId);
    BairroResponse Create(BairroRequest request);
    BairroResponse? Update(Guid id, BairroRequest request);
    bool Delete(Guid id);
}