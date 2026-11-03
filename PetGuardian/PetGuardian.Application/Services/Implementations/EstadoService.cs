using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de estados (UF).
/// </summary>
public sealed class EstadoService(
    IRepository<Estado>    estadoRepository,
    ILogger<EstadoService> logger) : IEstadoService
{
    public IReadOnlyList<EstadoResponse> GetAll() =>
        estadoRepository.GetAll().Select(EstadoResponse.FromDomain).ToList();

    public EstadoResponse? GetById(Guid id) =>
        estadoRepository.GetById(id) is { } estado ? EstadoResponse.FromDomain(estado) : null;

    public EstadoResponse Create(EstadoRequest request)
    {
        var estado = estadoRepository.Add(request.ToDomain());
        logger.LogInformation("Estado {EstadoId} ('{NomeEstado}') cadastrado.", estado.Id, estado.NomeEstado);
        return EstadoResponse.FromDomain(estado);
    }

    public EstadoResponse? Update(Guid id, EstadoRequest request)
    {
        var estado = estadoRepository.GetById(id);
        if (estado is null)
            return null;

        estado.Atualizar(request.NomeEstado);
        estadoRepository.Update(estado);
        return EstadoResponse.FromDomain(estado);
    }

    public bool Delete(Guid id) => estadoRepository.Delete(id);
}
