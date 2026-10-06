using System.Collections.Concurrent;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;

namespace PetGuardian.IntegrationTests.Fixtures;

/// <summary>Implementação em memória do contrato do catálogo, para testes sem servidor MongoDB.</summary>
public sealed class InMemoryTrilhaCatalogoRepository : ITrilhaCatalogoRepository
{
    private readonly ConcurrentDictionary<Guid, TrilhaCatalogoResponse> _store = new();

    public Task<(int Inseridas, int Atualizadas)> UpsertAsync(
        IReadOnlyCollection<TrilhaCatalogoResponse> trilhas, CancellationToken cancellationToken = default)
    {
        int inseridas = 0, atualizadas = 0;
        foreach (var t in trilhas)
        {
            if (_store.ContainsKey(t.TrilhaIdOrigem)) atualizadas++; else inseridas++;
            _store[t.TrilhaIdOrigem] = t with { Id = t.Id ?? Guid.NewGuid().ToString("N") };
        }
        return Task.FromResult((inseridas, atualizadas));
    }

    public Task<long> RemoverAusentesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        long removidas = 0;
        foreach (var chave in _store.Keys.Where(k => !ids.Contains(k)).ToList())
            if (_store.TryRemove(chave, out _)) removidas++;
        return Task.FromResult(removidas);
    }

    public Task<TrilhaCatalogoResponse?> GetByTrilhaIdAsync(Guid trilhaId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(trilhaId, out var t) ? t : null);

    public Task<PagedResult<TrilhaCatalogoResponse>> BuscarAsync(
        TrilhaCatalogoFilter filter, string? sortBy, bool descending, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Termo))
            query = query.Where(t =>
                t.Nome.Contains(filter.Termo, StringComparison.OrdinalIgnoreCase) ||
                t.Descricao.Contains(filter.Termo, StringComparison.OrdinalIgnoreCase));
        if (filter.PetId.HasValue)
            query = query.Where(t => t.PetAlvo.PetId == filter.PetId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Dificuldade))
            query = query.Where(t => t.Modulos.Any(m => m.Aulas.Any(a =>
                a.Dificuldade.Equals(filter.Dificuldade, StringComparison.OrdinalIgnoreCase))));

        var ordenada = (descending ? query.OrderByDescending(t => t.Nome) : query.OrderBy(t => t.Nome)).ToList();
        var itens = ordenada.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<TrilhaCatalogoResponse>(itens, ordenada.Count));
    }
}