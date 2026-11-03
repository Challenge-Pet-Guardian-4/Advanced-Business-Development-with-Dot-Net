using System.Linq.Expressions;
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
    IPetRepository       petRepository,
    IRepository<Raca>    racaRepository,
    ITarefaRepository    tarefaRepository,
    IHistoricoRepository historicoRepository,
    ILogger<PetService>  logger) : IPetService
{
    public PagedResponse<PetResponse> Search(PageQuery query, PetFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.Search");
        var q = query.Normalized();

        var nome = filter.Nome?.Trim().ToLowerInvariant();
        Expression<Func<Pet, bool>> predicate = p =>
            (nome == null || p.Nome.ToLower().Contains(nome))
            && (filter.RacaId == null || p.RacaId == filter.RacaId)
            && (filter.Porte == null || p.Porte == filter.Porte)
            && (filter.Sexo == null || p.Sexo == filter.Sexo)
            && (filter.Castrado == null || p.Castrado == filter.Castrado);

        var resultado = petRepository.GetPaged(predicate, q.SortBy ?? nameof(Pet.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<PetResponse>.Create(resultado.Map(PetResponse.FromDomain), q);
    }

    public PetResponse? GetById(Guid id) =>
        petRepository.GetById(id) is { } pet ? PetResponse.FromDomain(pet) : null;

    public IReadOnlyList<PetResponse> GetByIds(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        return idSet.Count == 0
            ? []
            : petRepository.Find(p => idSet.Contains(p.Id)).Select(PetResponse.FromDomain).ToList();
    }

    public IReadOnlyList<PetResponse> GetByRacaId(Guid racaId) =>
        petRepository.GetByRacaId(racaId).Select(PetResponse.FromDomain).ToList();

    public IReadOnlyList<PetHistoricoItemResponse> GetHistorico(Guid petId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.GetHistorico");
        activity?.SetTag("pet.id", petId.ToString());

        if (!petRepository.ExistsById(petId))
            throw new InvalidOperationException("Pet não encontrado.");

        var eventos = historicoRepository.GetByPetId(petId)
            .Select(h => new PetHistoricoItemResponse(h.DataHist, h.TipoHist, h.Id, h.TipoHist, null, h.PetId, null, null))
            .Concat(tarefaRepository.GetByPetId(petId)
                .Where(t => t.Conclusao.HasValue)
                .Select(t => new PetHistoricoItemResponse(
                    t.Conclusao!.Value, "TAREFA_CONCLUIDA", t.Id, t.Titulo, t.Descricao, t.PetId, t.UsuarioId, t.PontosTarefa)))
            .OrderByDescending(i => i.DataEvento)
            .ToList();

        return eventos;
    }

    public PetResponse Create(PetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.Create");
        activity?.SetTag("pet.nome", request.Nome);
        activity?.SetTag("pet.racaId", request.RacaId.ToString());

        if (!racaRepository.ExistsById(request.RacaId))
            throw new InvalidOperationException("Raça não encontrada.");

        var pet = petRepository.Add(request.ToDomain());
        logger.LogInformation("Pet {PetId} ({Nome}) cadastrado com sucesso.", pet.Id, pet.Nome);
        return PetResponse.FromDomain(pet);
    }

    public PetResponse? Update(Guid id, PetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("PetService.Update");
        activity?.SetTag("pet.id", id.ToString());

        var pet = petRepository.GetById(id);
        if (pet is null)
            return null;

        if (!racaRepository.ExistsById(request.RacaId))
            throw new InvalidOperationException("Raça não encontrada.");

        pet.Atualizar(request.Nome, request.DataNascimento, request.Sexo, request.Porte, request.Castrado, request.RacaId);
        petRepository.Update(pet);
        return PetResponse.FromDomain(pet);
    }

    public bool Delete(Guid id) => petRepository.Delete(id);
}
