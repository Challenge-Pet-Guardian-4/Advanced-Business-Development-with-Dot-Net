using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface ITrilhaCatalogoService
{
    /// <summary>Exporta Trilha→Módulo→Aula do relacional para o MongoDB (idempotente).</summary>
    Task<SincronizacaoCatalogoResponse> SincronizarAsync(CancellationToken cancellationToken = default);

    Task<PagedResponse<TrilhaCatalogoResponse>> BuscarAsync(
        PageQuery query, TrilhaCatalogoFilter filter, CancellationToken cancellationToken = default);

    Task<TrilhaCatalogoResponse?> GetByTrilhaIdAsync(Guid trilhaId, CancellationToken cancellationToken = default);
}