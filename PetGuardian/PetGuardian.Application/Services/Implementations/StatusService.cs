using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de status do ciclo de vida de tarefas.
/// </summary>
public sealed class StatusService(
    IRepository<Status>    statusRepository,
    ILogger<StatusService> logger) : IStatusService
{
    public IReadOnlyList<StatusResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os status de tarefas cadastrados.");
        return statusRepository.GetAll().Select(StatusResponse.FromDomain).ToList();
    }

    public StatusResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando status por ID: {StatusId}", id);
        var status = statusRepository.GetById(id);
        if (status is null)
            logger.LogWarning("Status com ID {StatusId} não encontrado.", id);

        return status is null ? null : StatusResponse.FromDomain(status);
    }

    public StatusResponse Create(StatusRequest request)
    {
        logger.LogInformation("Iniciando cadastro de status: {NomeStatus}", request.NomeStatus);
        var status = request.ToDomain();
        statusRepository.Add(status);
        logger.LogInformation("Status {StatusId} ('{NomeStatus}') cadastrado com sucesso.", status.Id, status.NomeStatus);
        return StatusResponse.FromDomain(status);
    }
    
    public StatusResponse? Update(Guid id, StatusRequest request)
    {
        logger.LogInformation("Iniciando atualização do status: {StatusId}", id);
        var status = statusRepository.GetById(id);
        if (status is null)
        {
            logger.LogWarning("Tentativa de atualizar status inexistente: {StatusId}", id);
            return null;
        }

        status.Atualizar(request.NomeStatus);
        statusRepository.Update(status);
        logger.LogInformation("Status {StatusId} atualizado com sucesso.", id);
        return StatusResponse.FromDomain(status);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do status: {StatusId}", id);
        var removido = statusRepository.Delete(id);
        if (removido)
            logger.LogInformation("Status {StatusId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de status inexistente: {StatusId}", id);

        return removido;
    }
}