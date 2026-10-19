using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de aulas de capacitação vinculadas a módulos.
/// </summary>
public sealed class AulaService(
    IAulaRepository         aulaRepository,
    IModuloRepository       moduloRepository,
    IConteudoAulaRepository conteudoRepository,
    ILogger<AulaService>    logger) : IAulaService
{
    public IReadOnlyList<AulaResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as aulas cadastradas.");
        var aulas = aulaRepository.GetAll();
        var conteudos = BuscarConteudosEmLote(aulas);
        return aulas.Select(a => AulaResponse.FromDomain(a, conteudos.GetValueOrDefault(a.Id))).ToList();
    }

    public AulaResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando aula por ID: {AulaId}", id);
        var aula = aulaRepository.GetById(id);
        if (aula is null)
        {
            logger.LogWarning("Aula com ID {AulaId} não encontrada.", id);
            return null;
        }

        return AulaResponse.FromDomain(aula, BuscarConteudoMarkdown(id));
    }

    public IReadOnlyList<AulaResponse> GetByModuloId(Guid moduloId)
    {
        logger.LogInformation("Buscando aulas do módulo: {ModuloId}", moduloId);
        var aulas = aulaRepository.GetByModuloId(moduloId);
        var conteudos = BuscarConteudosEmLote(aulas);
        return aulas.Select(a => AulaResponse.FromDomain(a, conteudos.GetValueOrDefault(a.Id))).ToList();
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

        aula.Atualizar(request.Nome, request.Descricao, request.PontosAula, request.Dificuldade, request.Concluida);
        aulaRepository.Update(aula);
        logger.LogInformation("Aula {AulaId} atualizada com sucesso.", id);
        return AulaResponse.FromDomain(aula, BuscarConteudoMarkdown(id));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Iniciando exclusão da aula: {AulaId}", id);
        if (!aulaRepository.Delete(id))
        {
            logger.LogWarning("Tentativa de exclusão de aula inexistente: {AulaId}", id);
            return false;
        }

        logger.LogInformation("Aula {AulaId} removida com sucesso.", id);
        try
        {
            await conteudoRepository.ExcluirPorAulaIdAsync(id, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Aula {AulaId} excluída, mas o conteúdo no MongoDB não pôde ser removido.", id);
        }
        return true;
    }

    private string? BuscarConteudoMarkdown(Guid aulaId)
    {
        try
        {
            return conteudoRepository.GetByAulaIdAsync(aulaId).GetAwaiter().GetResult()?.CorpoMarkdown;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar conteúdo no MongoDB para a aula {AulaId}.", aulaId);
            return null;
        }
    }

    private IReadOnlyDictionary<Guid, string> BuscarConteudosEmLote(IReadOnlyList<Aula> aulas)
    {
        if (aulas.Count == 0)
            return new Dictionary<Guid, string>();

        try
        {
            var docs = conteudoRepository.GetByAulaIdsAsync(aulas.Select(a => a.Id).ToList()).GetAwaiter().GetResult();
            return docs.ToDictionary(kv => kv.Key, kv => kv.Value.CorpoMarkdown);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar conteúdos em lote no MongoDB.");
            return new Dictionary<Guid, string>();
        }
    }
}