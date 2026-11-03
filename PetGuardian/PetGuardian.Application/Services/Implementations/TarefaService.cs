using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de tarefas de cuidado, validação de cuidadores e auditoria de conclusão.
/// </summary>
public sealed class TarefaService(
    ITarefaRepository      tarefaRepository,
    IPetRepository         petRepository,
    IRepository<Status>    statusRepository,
    IUsuarioRepository     usuarioRepository,
    IUsuarioPetRepository  usuarioPetRepository,
    IHistoricoRepository   historicoRepository,
    ILogger<TarefaService> logger,
    IUnitOfWork?           unitOfWork = null) : ITarefaService
{
    public PagedResponse<TarefaResponse> Search(PageQuery query, TarefaFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Search");
        var q = query.Normalized();

        var titulo = filter.Titulo?.Trim().ToLowerInvariant();
        Expression<Func<Tarefa, bool>> predicate = t =>
            (titulo == null || t.Titulo.ToLower().Contains(titulo))
            && (filter.PetId == null || t.PetId == filter.PetId)
            && (filter.UsuarioId == null || t.UsuarioId == filter.UsuarioId)
            && (filter.StatusId == null || t.StatusId == filter.StatusId)
            && (filter.Concluida == null || (t.Conclusao != null) == filter.Concluida)
            && (filter.PrazoDe == null || t.Prazo >= filter.PrazoDe)
            && (filter.PrazoAte == null || t.Prazo <= filter.PrazoAte);

        var resultado = tarefaRepository.GetPaged(predicate, q.SortBy ?? nameof(Tarefa.Prazo), q.Descending, q.Page, q.PageSize);
        return PagedResponse<TarefaResponse>.Create(resultado.Map(TarefaResponse.FromDomain), q);
    }

    public TarefaResponse? GetById(Guid id) =>
        tarefaRepository.GetById(id) is { } t ? TarefaResponse.FromDomain(t) : null;

    public IReadOnlyList<TarefaResponse> GetMinhasTarefas(Guid usuarioId, bool? concluida = null)
    {
        var petIds = usuarioPetRepository.GetByUsuarioId(usuarioId).Select(v => v.PetId).ToHashSet();
        return tarefaRepository
            .Find(t => (t.UsuarioId == usuarioId || petIds.Contains(t.PetId))
                       && (concluida == null || (t.Conclusao != null) == concluida.Value))
            .Select(TarefaResponse.FromDomain)
            .ToList();
    }

    public IReadOnlyList<TarefaResponse> GetByPetId(Guid petId) =>
        tarefaRepository.GetByPetId(petId).Select(TarefaResponse.FromDomain).ToList();

    public IReadOnlyList<TarefaResponse> GetByUsuarioId(Guid usuarioId) =>
        tarefaRepository.GetByUsuarioId(usuarioId).Select(TarefaResponse.FromDomain).ToList();

    public IReadOnlyList<TarefaResponse> GetByStatusId(Guid statusId) =>
        tarefaRepository.GetByStatusId(statusId).Select(TarefaResponse.FromDomain).ToList();

    public TarefaResponse Create(TarefaRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Create");
        activity?.SetTag("tarefa.petId", request.PetId.ToString());
        activity?.SetTag("tarefa.usuarioId", request.UsuarioId.ToString());

        if (!petRepository.ExistsById(request.PetId))
            throw new InvalidOperationException("Pet não encontrado.");

        if (!usuarioRepository.ExistsById(request.UsuarioId))
            throw new InvalidOperationException("Usuário não encontrado.");

        if (!usuarioPetRepository.Exists(request.UsuarioId, request.PetId))
            throw new InvalidOperationException("Somente cuidadores vinculados ao pet podem receber tarefas.");

        var statusPendente = BuscarStatusObrigatorio("PENDENTE");
        var tarefa = tarefaRepository.Add(request.ToDomain(statusPendente.Id));
        logger.LogInformation("Tarefa {TarefaId} ('{Titulo}') criada para o pet {PetId}.", tarefa.Id, tarefa.Titulo, tarefa.PetId);
        return TarefaResponse.FromDomain(tarefa);
    }

    public TarefaResponse? Update(Guid id, TarefaUpdateRequest request)
    {
        var tarefa = tarefaRepository.GetById(id);
        if (tarefa is null)
            return null;

        tarefa.Atualizar(request.Titulo, request.PontosTarefa, request.Descricao, request.Prazo);
        tarefaRepository.Update(tarefa);
        return TarefaResponse.FromDomain(tarefa);
    }

    public TarefaResponse Concluir(Guid tarefaId, Guid usuarioId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Concluir");
        activity?.SetTag("tarefa.id", tarefaId.ToString());
        activity?.SetTag("tarefa.usuarioId", usuarioId.ToString());

        var tarefa = ObterTarefaValidaParaCuidador(tarefaId, usuarioId, "concluir");
        if (tarefa.Conclusao.HasValue)
            throw new InvalidOperationException("A tarefa já foi concluída.");

        var statusConcluido = BuscarStatusObrigatorio("CONCLUIDO");

        return unitOfWork.RunInTransaction(() =>
        {
            tarefa.AtualizarStatus(statusConcluido.Id);
            tarefa.Concluir();
            tarefaRepository.Update(tarefa);
            historicoRepository.Add(Historico.Registrar("TAREFA_CONCLUIDA", tarefa.PetId));

            logger.LogInformation("Tarefa {TarefaId} concluída pelo usuário {UsuarioId} ({Pontos} pts).",
                tarefaId, usuarioId, tarefa.PontosTarefa);
            return TarefaResponse.FromDomain(tarefa);
        });
    }

    public TarefaResponse Desmarcar(Guid tarefaId, Guid usuarioId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Desmarcar");
        activity?.SetTag("tarefa.id", tarefaId.ToString());
        activity?.SetTag("tarefa.usuarioId", usuarioId.ToString());

        var tarefa = ObterTarefaValidaParaCuidador(tarefaId, usuarioId, "desmarcar");
        var statusPendente = BuscarStatusObrigatorio("PENDENTE");

        tarefa.Desmarcar(statusPendente.Id);
        tarefaRepository.Update(tarefa);
        logger.LogInformation("Tarefa {TarefaId} revertida para PENDENTE pelo usuário {UsuarioId}.", tarefaId, usuarioId);
        return TarefaResponse.FromDomain(tarefa);
    }

    public bool Delete(Guid id) => tarefaRepository.Delete(id);

    private Tarefa ObterTarefaValidaParaCuidador(Guid tarefaId, Guid usuarioId, string acao)
    {
        var tarefa = tarefaRepository.GetById(tarefaId)
            ?? throw new InvalidOperationException("Tarefa não encontrada.");

        if (!usuarioPetRepository.Exists(usuarioId, tarefa.PetId))
            throw new InvalidOperationException($"Somente cuidadores vinculados ao pet podem {acao} a tarefa.");

        return tarefa;
    }

    private Status BuscarStatusObrigatorio(string nomeStatus)
    {
        var nomeLower = nomeStatus.ToLowerInvariant();
        return statusRepository.FirstOrDefault(s => s.NomeStatus.ToLower() == nomeLower)
               ?? throw new InvalidOperationException($"Status obrigatório não encontrado: {nomeStatus}.");
    }
}
