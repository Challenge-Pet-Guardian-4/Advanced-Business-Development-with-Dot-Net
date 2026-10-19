using System.Collections.Concurrent;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;

namespace PetGuardian.IntegrationTests.Fixtures;

/// <summary>Implementação em memória do contrato de conteúdos, para testes sem servidor MongoDB.</summary>
public sealed class InMemoryConteudoAulaRepository : IConteudoAulaRepository
{
    private readonly ConcurrentDictionary<Guid, ConteudoAulaResponse> _store = new();

    public Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(aulaId, out var c) ? c : null);

    public Task<IReadOnlyDictionary<Guid, ConteudoAulaResponse>> GetByAulaIdsAsync(
        IReadOnlyCollection<Guid> aulaIds, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<Guid, ConteudoAulaResponse>>(
            _store.Where(kv => aulaIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public Task<ConteudoAulaResponse> InserirAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        var salvo = new ConteudoAulaResponse(
            Guid.NewGuid().ToString("N"), aulaId, request.TipoConteudo.Trim(),
            request.CorpoMarkdown, request.LinksRecursos ?? [], DateTime.UtcNow);
        _store[aulaId] = salvo;
        return Task.FromResult(salvo);
    }

    public Task<ConteudoAulaResponse?> AtualizarAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(aulaId, out var existente))
            return Task.FromResult<ConteudoAulaResponse?>(null);

        var atualizado = new ConteudoAulaResponse(
            existente.Id, aulaId, request.TipoConteudo.Trim(),
            request.CorpoMarkdown, request.LinksRecursos ?? [], DateTime.UtcNow);
        _store[aulaId] = atualizado;
        return Task.FromResult<ConteudoAulaResponse?>(atualizado);
    }

    public Task<bool> ExcluirPorAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryRemove(aulaId, out _));
}
