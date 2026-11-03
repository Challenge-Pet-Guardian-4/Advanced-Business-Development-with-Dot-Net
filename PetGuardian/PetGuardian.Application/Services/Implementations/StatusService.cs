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
    public IReadOnlyList<StatusResponse> GetAll() =>
        statusRepository.GetAll().Select(StatusResponse.FromDomain).ToList();

    public StatusResponse? GetById(Guid id) =>
        statusRepository.GetById(id) is { } status ? StatusResponse.FromDomain(status) : null;

    public StatusResponse Create(StatusRequest request)
    {
        var status = statusRepository.Add(request.ToDomain());
        logger.LogInformation("Status {StatusId} ('{NomeStatus}') cadastrado.", status.Id, status.NomeStatus);
        return StatusResponse.FromDomain(status);
    }

    public StatusResponse? Update(Guid id, StatusRequest request)
    {
        var status = statusRepository.GetById(id);
        if (status is null)
            return null;

        status.Atualizar(request.NomeStatus);
        statusRepository.Update(status);
        return StatusResponse.FromDomain(status);
    }

    public bool Delete(Guid id) => statusRepository.Delete(id);
}
