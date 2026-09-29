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
    IRepository<Raca>     racaRepository,
    ILogger<RacaService>  logger) : IRacaService
{
    public IReadOnlyList<RacaResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as raças cadastradas.");
        return racaRepository.GetAll().Select(RacaResponse.FromDomain).ToList();
    }

    public RacaResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando raça por ID: {RacaId}", id);
        var raca = racaRepository.GetById(id);
        if (raca is null)
            logger.LogWarning("Raça com ID {RacaId} não encontrada.", id);

        return raca is null ? null : RacaResponse.FromDomain(raca);
    }

    public RacaResponse Create(RacaRequest request)
    {
        logger.LogInformation("Iniciando cadastro de raça: {NomeRaca}", request.NomeRaca);
        var raca = request.ToDomain();
        racaRepository.Add(raca);
        logger.LogInformation("Raça {RacaId} ('{NomeRaca}') cadastrada com sucesso.", raca.Id, raca.NomeRaca);
        return RacaResponse.FromDomain(raca);
    }

    public RacaResponse? Update(Guid id, RacaRequest request)
    {
        logger.LogInformation("Iniciando atualização da raça: {RacaId}", id);
        var raca = racaRepository.GetById(id);
        if (raca is null)
        {
            logger.LogWarning("Tentativa de atualizar raça inexistente: {RacaId}", id);
            return null;
        }

        raca.Atualizar(request.NomeRaca);
        racaRepository.Update(raca);
        logger.LogInformation("Raça {RacaId} atualizada com sucesso.", id);
        return RacaResponse.FromDomain(raca);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão da raça: {RacaId}", id);
        var removido = racaRepository.Delete(id);
        if (removido)
            logger.LogInformation("Raça {RacaId} removida com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de raça inexistente: {RacaId}", id);

        return removido;
    }
}