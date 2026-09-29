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
    IRepository<Estado>     estadoRepository,
    ILogger<EstadoService>  logger) : IEstadoService
{
    public IReadOnlyList<EstadoResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os estados cadastrados.");
        return estadoRepository.GetAll().Select(EstadoResponse.FromDomain).ToList();
    }

    public EstadoResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando estado por ID: {EstadoId}", id);
        var estado = estadoRepository.GetById(id);
        if (estado is null)
            logger.LogWarning("Estado com ID {EstadoId} não encontrado.", id);

        return estado is null ? null : EstadoResponse.FromDomain(estado);
    }

    public EstadoResponse Create(EstadoRequest request)
    {
        logger.LogInformation("Iniciando cadastro de estado: {NomeEstado}", request.NomeEstado);
        var estado = request.ToDomain();
        estadoRepository.Add(estado);
        logger.LogInformation("Estado {EstadoId} ('{NomeEstado}') cadastrado com sucesso.", estado.Id, estado.NomeEstado);
        return EstadoResponse.FromDomain(estado);
    }
    
    public EstadoResponse? Update(Guid id, EstadoRequest request)
    {
        logger.LogInformation("Iniciando atualização do estado: {EstadoId}", id);
        var estado = estadoRepository.GetById(id);
        if (estado is null)
        {
            logger.LogWarning("Tentativa de atualizar estado inexistente: {EstadoId}", id);
            return null;
        }

        estado.Atualizar(request.NomeEstado);
        estadoRepository.Update(estado);
        logger.LogInformation("Estado {EstadoId} atualizado com sucesso.", id);
        return EstadoResponse.FromDomain(estado);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do estado: {EstadoId}", id);
        var removido = estadoRepository.Delete(id);
        if (removido)
            logger.LogInformation("Estado {EstadoId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de estado inexistente: {EstadoId}", id);

        return removido;
    }
}