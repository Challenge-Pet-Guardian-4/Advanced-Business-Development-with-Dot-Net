using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IConteudoAulaService
{
    Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default);
    Task<ConteudoAulaResponse> CreateAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default);
    Task<ConteudoAulaResponse?> UpdateAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid aulaId, CancellationToken cancellationToken = default);
}
