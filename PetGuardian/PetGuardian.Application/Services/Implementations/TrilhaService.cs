using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de trilhas de capacitação vinculadas a pets.
/// </summary>
public sealed class TrilhaService(
    ITrilhaRepository      trilhaRepository,
    IPetRepository         petRepository,
    ILogger<TrilhaService> logger) : ITrilhaService
{
    public PagedResponse<TrilhaResponse> Search(PageQuery query, TrilhaFilter filter)
    {
        var q = query.Normalized();
        var nome = filter.Nome?.Trim().ToLowerInvariant();
        Expression<Func<Trilha, bool>> predicate = t =>
            (nome == null || t.Nome.ToLower().Contains(nome))
            && (filter.PetId == null || t.PetId == filter.PetId);

        var resultado = trilhaRepository.GetPaged(predicate, q.SortBy ?? nameof(Trilha.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<TrilhaResponse>.Create(resultado.Map(TrilhaResponse.FromDomain), q);
    }

    public TrilhaResponse? GetById(Guid id) =>
        trilhaRepository.GetById(id) is { } trilha ? TrilhaResponse.FromDomain(trilha) : null;

    public IReadOnlyList<TrilhaResponse> GetByPetId(Guid petId) =>
        trilhaRepository.GetByPetId(petId).Select(TrilhaResponse.FromDomain).ToList();

    public IReadOnlyList<TrilhaResponse> GetByPetIds(IEnumerable<Guid> petIds)
    {
        var idSet = petIds.ToHashSet();
        return idSet.Count == 0
            ? []
            : trilhaRepository.Find(t => idSet.Contains(t.PetId)).Select(TrilhaResponse.FromDomain).ToList();
    }

    public TrilhaResponse Create(TrilhaRequest request)
    {
        if (!petRepository.ExistsById(request.PetId))
            throw new InvalidOperationException("Pet não encontrado.");

        var trilha = trilhaRepository.Add(request.ToDomain());
        logger.LogInformation("Trilha {TrilhaId} ('{Nome}') cadastrada para o pet {PetId}.", trilha.Id, trilha.Nome, trilha.PetId);
        return TrilhaResponse.FromDomain(trilha);
    }

    public TrilhaResponse? Update(Guid id, TrilhaUpdateRequest request)
    {
        var trilha = trilhaRepository.GetById(id);
        if (trilha is null)
            return null;

        trilha.Atualizar(request.Nome, request.Descricao);
        trilhaRepository.Update(trilha);
        return TrilhaResponse.FromDomain(trilha);
    }

    public bool Delete(Guid id) => trilhaRepository.Delete(id);
}
