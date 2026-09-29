using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de aulas de capacitação vinculadas a módulos.
/// </summary>
public sealed class AulaService(
    IAulaRepository       aulaRepository,
    IModuloRepository     moduloRepository,
    ILogger<AulaService>  logger) : IAulaService
{
    public IReadOnlyList<AulaResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as aulas cadastradas.");
        return aulaRepository.GetAll().Select(AulaResponse.FromDomain).ToList();
    }

    public AulaResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando aula por ID: {AulaId}", id);
        var aula = aulaRepository.GetById(id);
        if (aula is null)
            logger.LogWarning("Aula com ID {AulaId} não encontrada.", id);

        return aula is null ? null : AulaResponse.FromDomain(aula);
    }

    public IReadOnlyList<AulaResponse> GetByModuloId(Guid moduloId)
    {
        logger.LogInformation("Buscando aulas do módulo: {ModuloId}", moduloId);
        return aulaRepository.GetByModuloId(moduloId).Select(AulaResponse.FromDomain).ToList();
    }

    public AulaResponse Create(AulaRequest request)
    {
        logger.LogInformation("Iniciando cadastro de aula '{Nome}' para o módulo {ModuloId}", request.Nome, request.ModuloId);
        if (!moduloRepository.ExistsById(request.ModuloId))
        {
            logger.LogWarning("Tentativa de criar aula para módulo inexistente: {ModuloId}", request.ModuloId);
            throw new InvalidOperationException("Módulo não encontrado.");
        }

        var aula = request.ToDomain();
        aulaRepository.Add(aula);
        logger.LogInformation("Aula {AulaId} ('{Nome}') cadastrada com sucesso.", aula.Id, aula.Nome);
        return AulaResponse.FromDomain(aula);
    }

    public AulaResponse? Update(Guid id, AulaUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização da aula: {AulaId}", id);
        var aula = aulaRepository.GetById(id);
        if (aula is null)
        {
            logger.LogWarning("Tentativa de atualizar aula inexistente: {AulaId}", id);
            return null;
        }

        aula.Atualizar(request.Nome, request.Descricao, request.PontosAula, request.Dificuldade, request.Conteudo, request.Concluida);
        aulaRepository.Update(aula);
        logger.LogInformation("Aula {AulaId} atualizada com sucesso.", id);
        return AulaResponse.FromDomain(aula);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão da aula: {AulaId}", id);
        var removido = aulaRepository.Delete(id);
        if (removido)
            logger.LogInformation("Aula {AulaId} removida com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de aula inexistente: {AulaId}", id);

        return removido;
    }
}