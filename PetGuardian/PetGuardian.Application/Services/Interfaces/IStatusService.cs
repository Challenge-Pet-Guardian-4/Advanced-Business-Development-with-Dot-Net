using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IStatusService
{
    IReadOnlyList<StatusResponse> GetAll();
    StatusResponse? GetById(Guid id);
    StatusResponse Create(StatusRequest request);
    StatusResponse? Update(Guid id, StatusRequest request);
    bool Delete(Guid id);
}