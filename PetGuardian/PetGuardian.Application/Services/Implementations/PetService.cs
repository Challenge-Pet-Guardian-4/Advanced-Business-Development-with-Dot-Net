using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de pets, histórico integrado e linha do tempo de cuidados.
/// </summary>
public sealed class PetService(
    IPetRepository        petRepository,
    IRepository<Raca>     racaRepository,
    ITarefaRepository     tarefaRepository,
    IHistoricoRepository  historicoRepository,
    ILogger<PetService>   logger) : IPetService
{
    public IReadOnlyList<PetResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os pets cadastrados.");
        return petRepository.GetAll().Select(PetResponse.FromDomain).ToList();
    }

    public PetResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando pet por ID: {PetId}", id);
        var pet = petRepository.GetById(id);
        if (pet is null)
            logger.LogWarning("Pet com ID {PetId} não encontrado.", id);

        return pet is null ? null : PetResponse.FromDomain(pet);
    }

    public IReadOnlyList<PetResponse> GetByRacaId(Guid racaId)
    {
        logger.LogInformation("Buscando pets por raça: {RacaId}", racaId);
        return petRepository.GetByRacaId(racaId)
            .Select(PetResponse.FromDomain)
            .ToList();
    }

    public IReadOnlyList<PetHistoricoItemResponse> GetHistorico(Guid petId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.GetHistorico");
        activity?.SetTag("pet.id", petId.ToString());

        logger.LogInformation("Buscando linha do tempo histórica do pet: {PetId}", petId);
        if (!petRepository.ExistsById(petId))
        {
            logger.LogWarning("Tentativa de obter histórico de pet inexistente: {PetId}", petId);
            throw new InvalidOperationException("Pet não encontrado.");
        }

        var historico = new List<PetHistoricoItemResponse>();

        historico.AddRange(historicoRepository.GetByPetId(petId)
            .Select(h => new PetHistoricoItemResponse(
                h.DataHist,
                h.TipoHist,
                h.Id,
                h.TipoHist,
                null,
                h.PetId,
                null,
                null)));

        historico.AddRange(tarefaRepository.GetByPetId(petId)
            .Where(t => t.Conclusao.HasValue)
            .Select(t => new PetHistoricoItemResponse(
                t.Conclusao!.Value,
                "TAREFA_CONCLUIDA",
                t.Id,
                t.Titulo,
                t.Descricao,
                t.PetId,
                t.UsuarioId,
                t.PontosTarefa)));

        var historicoOrdenado = historico
            .OrderByDescending(i => i.DataEvento)
            .ToList();

        logger.LogInformation("Linha do tempo consolidada para o pet {PetId}: {TotalEventos} eventos.", petId, historicoOrdenado.Count);
        return historicoOrdenado;
    }

    public PetResponse Create(PetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.Create");
        activity?.SetTag("pet.nome", request.Nome);
        activity?.SetTag("pet.racaId", request.RacaId.ToString());

        logger.LogInformation("Iniciando cadastro do pet {Nome}, Raça: {RacaId}, Porte: {Porte}, Sexo: {Sexo}",
            request.Nome, request.RacaId, request.Porte, request.Sexo);

        if (!racaRepository.ExistsById(request.RacaId))
        {
            logger.LogWarning("Tentativa de cadastrar pet com raça inexistente: {RacaId}", request.RacaId);
            throw new InvalidOperationException("Raça não encontrada.");
        }

        var pet = request.ToDomain();
        petRepository.Add(pet);
        logger.LogInformation("Pet {PetId} ({Nome}) cadastrado com sucesso.", pet.Id, pet.Nome);
        return PetResponse.FromDomain(pet);
    }
    
    public PetResponse? Update(Guid id, PetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.Update");
        activity?.SetTag("pet.id", id.ToString());

        logger.LogInformation("Iniciando atualização do pet: {PetId}", id);
        var pet = petRepository.GetById(id);
        if (pet is null)
        {
            logger.LogWarning("Tentativa de atualizar pet inexistente: {PetId}", id);
            return null;
        }

        if (!racaRepository.ExistsById(request.RacaId))
        {
            logger.LogWarning("Tentativa de atualizar pet com raça inexistente: {RacaId}", request.RacaId);
            throw new InvalidOperationException("Raça não encontrada.");
        }

        pet.Atualizar(request.Nome, request.DataNascimento, request.Sexo, request.Porte, request.Castrado, request.RacaId);
        petRepository.Update(pet);
        logger.LogInformation("Pet {PetId} ({Nome}) atualizado com sucesso.", id, pet.Nome);
        return PetResponse.FromDomain(pet);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão de pet: {PetId}", id);
        var removido = petRepository.Delete(id);
        if (removido)
            logger.LogInformation("Pet {PetId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de pet inexistente: {PetId}", id);

        return removido;
    }
}
