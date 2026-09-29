using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo registro e consulta de eventos da linha do tempo histórica de pets.
/// </summary>
public sealed class HistoricoService(
    IHistoricoRepository        historicoRepository,
    IPetRepository              petRepository,
    ILogger<HistoricoService>   logger) : IHistoricoService
{
    public IReadOnlyList<HistoricoResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os registros históricos.");
        return historicoRepository.GetAll().Select(HistoricoResponse.FromDomain).ToList();
    }

    public HistoricoResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando histórico por ID: {HistoricoId}", id);
        var h = historicoRepository.GetById(id);
        if (h is null)
            logger.LogWarning("Histórico com ID {HistoricoId} não encontrado.", id);

        return h is null ? null : HistoricoResponse.FromDomain(h);
    }

    public IReadOnlyList<HistoricoResponse> GetByPetId(Guid petId)
    {
        logger.LogInformation("Buscando eventos históricos do pet: {PetId}", petId);
        return historicoRepository.GetByPetId(petId).Select(HistoricoResponse.FromDomain).ToList();
    }

    public HistoricoResponse Create(HistoricoRequest request)
    {
        logger.LogInformation("Registrando histórico '{TipoHist}' para o pet {PetId} na data {DataHist}",
            request.TipoHist, request.PetId, request.DataHist);

        if (!petRepository.ExistsById(request.PetId))
        {
            logger.LogWarning("Tentativa de registrar histórico para pet inexistente: {PetId}", request.PetId);
            throw new InvalidOperationException("Pet não encontrado.");
        }

        var historico = request.ToDomain();
        historicoRepository.Add(historico);
        logger.LogInformation("Histórico {HistoricoId} ('{TipoHist}') registrado com sucesso para o pet {PetId}.",
            historico.Id, historico.TipoHist, historico.PetId);

        return HistoricoResponse.FromDomain(historico);
    }

    public HistoricoResponse? Update(Guid id, HistoricoUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização do histórico: {HistoricoId}", id);
        var historico = historicoRepository.GetById(id);
        if (historico is null)
        {
            logger.LogWarning("Tentativa de atualizar histórico inexistente: {HistoricoId}", id);
            return null;
        }

        historico.Atualizar(request.TipoHist, request.DataHist);
        historicoRepository.Update(historico);
        logger.LogInformation("Histórico {HistoricoId} atualizado com sucesso.", id);
        return HistoricoResponse.FromDomain(historico);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do histórico: {HistoricoId}", id);
        var removido = historicoRepository.Delete(id);
        if (removido)
            logger.LogInformation("Histórico {HistoricoId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de histórico inexistente: {HistoricoId}", id);

        return removido;
    }
}