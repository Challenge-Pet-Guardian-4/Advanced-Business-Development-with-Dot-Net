using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IAulaService
{
    IReadOnlyList<AulaResponse> GetAll();
    AulaResponse? GetById(Guid id);
    IReadOnlyList<AulaResponse> GetByModuloId(Guid moduloId);
    AulaResponse Create(AulaRequest request);
    AulaResponse? Update(Guid id, AulaUpdateRequest request);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}