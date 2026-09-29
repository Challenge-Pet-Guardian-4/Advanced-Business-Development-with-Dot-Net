using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de cidades vinculadas a estados.
/// </summary>
public sealed class CidadeService(
    IRepository<Cidade>     cidadeRepository,
    IRepository<Estado>     estadoRepository,
    ILogger<CidadeService>  logger) : ICidadeService
{
    public IReadOnlyList<CidadeResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as cidades cadastradas.");
        return cidadeRepository.GetAll().Select(CidadeResponse.FromDomain).ToList();
    }

    public CidadeResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando cidade por ID: {CidadeId}", id);
        var cidade = cidadeRepository.GetById(id);
        if (cidade is null)
            logger.LogWarning("Cidade com ID {CidadeId} não encontrada.", id);

        return cidade is null ? null : CidadeResponse.FromDomain(cidade);
    }

    public IReadOnlyList<CidadeResponse> GetByEstadoId(Guid estadoId)
    {
        logger.LogInformation("Buscando cidades do estado: {EstadoId}", estadoId);
        return cidadeRepository.Find(c => c.EstadoId == estadoId)
            .Select(CidadeResponse.FromDomain)
            .ToList();
    }

    public CidadeResponse Create(CidadeRequest request)
    {
        logger.LogInformation("Iniciando cadastro de cidade '{NomeCidade}' no estado {EstadoId}", request.NomeCidade, request.EstadoId);
        if (!estadoRepository.ExistsById(request.EstadoId))
        {
            logger.LogWarning("Tentativa de criar cidade para estado inexistente: {EstadoId}", request.EstadoId);
            throw new InvalidOperationException("Estado não encontrado.");
        }

        var cidade = request.ToDomain();
        cidadeRepository.Add(cidade);
        logger.LogInformation("Cidade {CidadeId} ('{NomeCidade}') cadastrada com sucesso.", cidade.Id, cidade.NomeCidade);
        return CidadeResponse.FromDomain(cidade);
    }
    
    public CidadeResponse? Update(Guid id, CidadeRequest request)
    {
        logger.LogInformation("Iniciando atualização da cidade: {CidadeId}", id);
        var cidade = cidadeRepository.GetById(id);
        if (cidade is null)
        {
            logger.LogWarning("Tentativa de atualizar cidade inexistente: {CidadeId}", id);
            return null;
        }

        if (!estadoRepository.ExistsById(request.EstadoId))
        {
            logger.LogWarning("Tentativa de atualizar cidade com estado inexistente: {EstadoId}", request.EstadoId);
            throw new InvalidOperationException("Estado não encontrado.");
        }

        cidade.Atualizar(request.NomeCidade, request.EstadoId);
        cidadeRepository.Update(cidade);
        logger.LogInformation("Cidade {CidadeId} atualizada com sucesso.", id);
        return CidadeResponse.FromDomain(cidade);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão da cidade: {CidadeId}", id);
        var removido = cidadeRepository.Delete(id);
        if (removido)
            logger.LogInformation("Cidade {CidadeId} removida com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de cidade inexistente: {CidadeId}", id);

        return removido;
    }
}