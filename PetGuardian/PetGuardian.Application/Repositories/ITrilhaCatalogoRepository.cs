using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Repositories;

/// <summary>Contrato do repositório NoSQL do catálogo de trilhas (a Application não conhece o driver).</summary>
public interface ITrilhaCatalogoRepository
{
    Task<(int Inseridas, int Atualizadas)> UpsertAsync(
        IReadOnlyCollection<TrilhaCatalogoResponse> trilhas, CancellationToken cancellationToken = default);

    Task<long> RemoverAusentesAsync(
        IReadOnlyCollection<Guid> trilhaIdsPresentes, CancellationToken cancellationToken = default);

    Task<TrilhaCatalogoResponse?> GetByTrilhaIdAsync(Guid trilhaId, CancellationToken cancellationToken = default);

    Task<PagedResult<TrilhaCatalogoResponse>> BuscarAsync(
        TrilhaCatalogoFilter filter, string? sortBy, bool descending, int page, int pageSize,
        CancellationToken cancellationToken = default);
}