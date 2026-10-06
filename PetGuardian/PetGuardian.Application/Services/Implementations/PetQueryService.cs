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
        var filtrarRaca = filter.RacaId.HasValue;       var racaId = filter.RacaId.GetValueOrDefault();
        var filtrarPorte = filter.Porte.HasValue;       var porte = filter.Porte.GetValueOrDefault();
        var filtrarSexo = filter.Sexo.HasValue;         var sexo = filter.Sexo.GetValueOrDefault();
        var filtrarCastrado = filter.Castrado.HasValue; var castrado = filter.Castrado.GetValueOrDefault();

        Expression<Func<Pet, bool>> predicate = p =>
            (nome == null || p.Nome.ToLower().Contains(nome))
            && (!filtrarRaca || p.RacaId == racaId)
            && (!filtrarPorte || p.Porte == porte)
            && (!filtrarSexo || p.Sexo == sexo)
            && (!filtrarCastrado || p.Castrado == castrado);

        var resultado = petRepository.GetPaged(predicate, q.SortBy ?? nameof(Pet.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<PetResponse>.Create(resultado.Map(PetResponse.FromDomain), q);
    }
}