using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

public sealed class PetQueryService(IPetRepository petRepository, ILogger<PetQueryService> logger) : IPetQueryService
{
    public PagedResponse<PetResponse> Search(PageQuery query, PetFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetQueryService.Search");
        var q = query.Normalized();
        logger.LogInformation("Pesquisando pets: página {Page}, tamanho {PageSize}, ordenação {SortBy} {SortDir}.",
            q.Page, q.PageSize, q.SortBy ?? nameof(Pet.Nome), q.SortDir);

        var nome = filter.Nome?.Trim().ToLowerInvariant();
        Expression<Func<Pet, bool>> predicate = p =>
            (nome == null || p.Nome.ToLower().Contains(nome))
            && (filter.RacaId == null || p.RacaId == filter.RacaId)
            && (filter.Porte == null || p.Porte == filter.Porte)
            && (filter.Sexo == null || p.Sexo == filter.Sexo)
            && (filter.Castrado == null || p.Castrado == filter.Castrado);

        var resultado = petRepository.GetPaged(predicate, q.SortBy ?? nameof(Pet.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<PetResponse>.Create(resultado.Map(PetResponse.FromDomain), q);
    }
}