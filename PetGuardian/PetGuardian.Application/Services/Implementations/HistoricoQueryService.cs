using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

public sealed class HistoricoQueryService(IHistoricoRepository historicoRepository, ILogger<HistoricoQueryService> logger) : IHistoricoQueryService
{
    public PagedResponse<HistoricoResponse> Search(PageQuery query, HistoricoFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("HistoricoQueryService.Search");
        var q = query.Normalized();
        logger.LogInformation("Pesquisando históricos: página {Page}, tamanho {PageSize}, ordenação {SortBy} {SortDir}.",
            q.Page, q.PageSize, q.SortBy ?? nameof(Historico.DataHist), q.SortDir);

        var tipo = filter.TipoHist?.Trim().ToLowerInvariant();
        var filtrarPet = filter.PetId.HasValue;   var petId = filter.PetId.GetValueOrDefault();
        var filtrarDe = filter.DataDe.HasValue;   var dataDe = filter.DataDe.GetValueOrDefault();
        var filtrarAte = filter.DataAte.HasValue; var dataAte = filter.DataAte.GetValueOrDefault();

        Expression<Func<Historico, bool>> predicate = h =>
            (tipo == null || h.TipoHist.ToLower().Contains(tipo))
            && (!filtrarPet || h.PetId == petId)
            && (!filtrarDe || h.DataHist >= dataDe)
            && (!filtrarAte || h.DataHist <= dataAte);

        var resultado = historicoRepository.GetPaged(predicate, q.SortBy ?? nameof(Historico.DataHist), q.Descending, q.Page, q.PageSize);
        return PagedResponse<HistoricoResponse>.Create(resultado.Map(HistoricoResponse.FromDomain), q);
    }
}