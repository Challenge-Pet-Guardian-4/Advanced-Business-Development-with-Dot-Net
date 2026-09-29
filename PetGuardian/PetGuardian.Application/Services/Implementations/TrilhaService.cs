using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de trilhas de capacitação vinculadas a pets.
/// </summary>
public sealed class TrilhaService(
    ITrilhaRepository        trilhaRepository,
    IPetRepository           petRepository,
    ILogger<TrilhaService>   logger) : ITrilhaService
{
    public IReadOnlyList<TrilhaResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as trilhas cadastradas.");
        return trilhaRepository.GetAll().Select(TrilhaResponse.FromDomain).ToList();
    }

    public TrilhaResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando trilha por ID: {TrilhaId}", id);
        var trilha = trilhaRepository.GetById(id);
        if (trilha is null)
            logger.LogWarning("Trilha com ID {TrilhaId} não encontrada.", id);

        return trilha is null ? null : TrilhaResponse.FromDomain(trilha);
    }

    public IReadOnlyList<TrilhaResponse> GetByPetId(Guid petId)
    {
        logger.LogInformation("Buscando trilhas do pet: {PetId}", petId);
        return trilhaRepository.GetByPetId(petId).Select(TrilhaResponse.FromDomain).ToList();
    }

    public TrilhaResponse Create(TrilhaRequest request)
    {
        logger.LogInformation("Iniciando cadastro de trilha '{Nome}' para o pet {PetId}", request.Nome, request.PetId);
        if (!petRepository.ExistsById(request.PetId))
        {
            logger.LogWarning("Tentativa de criar trilha para pet inexistente: {PetId}", request.PetId);
            throw new InvalidOperationException("Pet não encontrado.");
        }

        var trilha = request.ToDomain();
        trilhaRepository.Add(trilha);
        logger.LogInformation("Trilha {TrilhaId} ('{Nome}') cadastrada com sucesso.", trilha.Id, trilha.Nome);
        return TrilhaResponse.FromDomain(trilha);
    }

    public TrilhaResponse? Update(Guid id, TrilhaUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização da trilha: {TrilhaId}", id);
        var trilha = trilhaRepository.GetById(id);
        if (trilha is null)
        {
            logger.LogWarning("Tentativa de atualizar trilha inexistente: {TrilhaId}", id);
            return null;
        }

        trilha.Atualizar(request.Nome, request.Descricao);
        trilhaRepository.Update(trilha);
        logger.LogInformation("Trilha {TrilhaId} atualizada com sucesso.", id);
        return TrilhaResponse.FromDomain(trilha);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão da trilha: {TrilhaId}", id);
        var removido = trilhaRepository.Delete(id);
        if (removido)
            logger.LogInformation("Trilha {TrilhaId} removida com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de trilha inexistente: {TrilhaId}", id);

        return removido;
    }
}