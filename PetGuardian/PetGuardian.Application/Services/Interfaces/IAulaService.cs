using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IAulaService
{
    PagedResponse<AulaResponse> Search(PageQuery query, AulaFilter filter);
    AulaResponse? GetById(Guid id);
    IReadOnlyList<AulaResponse> GetByModuloId(Guid moduloId);
    AulaResponse Create(AulaRequest request);
    AulaResponse? Update(Guid id, AulaUpdateRequest request);
    AulaResponse? Concluir(Guid id);
    AulaResponse? Desmarcar(Guid id);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
