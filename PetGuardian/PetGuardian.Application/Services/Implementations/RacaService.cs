using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de raças de pets.
/// </summary>
public sealed class RacaService(
    IRepository<Raca>    racaRepository,
    ILogger<RacaService> logger) : IRacaService
{
    public IReadOnlyList<RacaResponse> GetAll() =>
        racaRepository.GetAll().Select(RacaResponse.FromDomain).ToList();

    public RacaResponse? GetById(Guid id) =>
        racaRepository.GetById(id) is { } raca ? RacaResponse.FromDomain(raca) : null;

    public RacaResponse Create(RacaRequest request)
    {
        var raca = racaRepository.Add(request.ToDomain());
        logger.LogInformation("Raça {RacaId} ('{NomeRaca}') cadastrada.", raca.Id, raca.NomeRaca);
        return RacaResponse.FromDomain(raca);
    }

    public RacaResponse? Update(Guid id, RacaRequest request)
    {
        var raca = racaRepository.GetById(id);
        if (raca is null)
            return null;

        raca.Atualizar(request.NomeRaca);
        racaRepository.Update(raca);
        return RacaResponse.FromDomain(raca);
    }

    public bool Delete(Guid id) => racaRepository.Delete(id);
}
