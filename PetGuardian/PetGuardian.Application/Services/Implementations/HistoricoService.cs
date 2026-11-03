using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo registro e consulta de eventos da linha do tempo histórica de pets.
/// </summary>
public sealed class HistoricoService(
    IHistoricoRepository      historicoRepository,
    IPetRepository            petRepository,
    ILogger<HistoricoService> logger) : IHistoricoService
{
    public PagedResponse<HistoricoResponse> Search(PageQuery query, HistoricoFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("HistoricoService.Search");
        var q = query.Normalized();

        var tipo = filter.TipoHist?.Trim().ToLowerInvariant();
        Expression<Func<Historico, bool>> predicate = h =>
            (tipo == null || h.TipoHist.ToLower().Contains(tipo))
            && (filter.PetId == null || h.PetId == filter.PetId)
            && (filter.DataDe == null || h.DataHist >= filter.DataDe)
            && (filter.DataAte == null || h.DataHist <= filter.DataAte);

        var resultado = historicoRepository.GetPaged(predicate, q.SortBy ?? nameof(Historico.DataHist), q.Descending, q.Page, q.PageSize);
        return PagedResponse<HistoricoResponse>.Create(resultado.Map(HistoricoResponse.FromDomain), q);
    }

    public HistoricoResponse? GetById(Guid id) =>
        historicoRepository.GetById(id) is { } h ? HistoricoResponse.FromDomain(h) : null;

    public IReadOnlyList<HistoricoResponse> GetByPetId(Guid petId) =>
        historicoRepository.GetByPetId(petId).Select(HistoricoResponse.FromDomain).ToList();

    public HistoricoResponse Create(HistoricoRequest request)
    {
        if (!petRepository.ExistsById(request.PetId))
            throw new InvalidOperationException("Pet não encontrado.");

        var historico = historicoRepository.Add(request.ToDomain());
        logger.LogInformation("Histórico {HistoricoId} ('{TipoHist}') registrado para o pet {PetId}.",
            historico.Id, historico.TipoHist, historico.PetId);
        return HistoricoResponse.FromDomain(historico);
    }

    public HistoricoResponse? Update(Guid id, HistoricoUpdateRequest request)
    {
        var historico = historicoRepository.GetById(id);
        if (historico is null)
            return null;

        historico.Atualizar(request.TipoHist, request.DataHist);
        historicoRepository.Update(historico);
        return HistoricoResponse.FromDomain(historico);
    }

    public bool Delete(Guid id) => historicoRepository.Delete(id);
}
