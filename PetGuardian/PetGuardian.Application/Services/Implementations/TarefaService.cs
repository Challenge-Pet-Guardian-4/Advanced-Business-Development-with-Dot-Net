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
    ITarefaRepository        tarefaRepository,
    IPetRepository           petRepository,
    IRepository<Status>      statusRepository,
    IUsuarioRepository       usuarioRepository,
    IUsuarioPetRepository    usuarioPetRepository,
    IHistoricoRepository     historicoRepository,
    ILogger<TarefaService>   logger) : ITarefaService
{
    public IReadOnlyList<TarefaResponse> GetAll()
    {
        logger.LogInformation("Buscando todas as tarefas cadastradas.");
        return tarefaRepository.GetAll().Select(TarefaResponse.FromDomain).ToList();
    }

    public TarefaResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando tarefa por ID: {TarefaId}", id);
        var t = tarefaRepository.GetById(id);
        if (t is null)
            logger.LogWarning("Tarefa com ID {TarefaId} não encontrada.", id);

        return t is null ? null : TarefaResponse.FromDomain(t);
    }

    public IReadOnlyList<TarefaResponse> GetByPetId(Guid petId)
    {
        logger.LogInformation("Buscando tarefas do pet: {PetId}", petId);
        return tarefaRepository.GetByPetId(petId).Select(TarefaResponse.FromDomain).ToList();
    }

    public IReadOnlyList<TarefaResponse> GetByUsuarioId(Guid usuarioId)
    {
        logger.LogInformation("Buscando tarefas do usuário responsável: {UsuarioId}", usuarioId);
        return tarefaRepository.GetByUsuarioId(usuarioId).Select(TarefaResponse.FromDomain).ToList();
    }

    public IReadOnlyList<TarefaResponse> GetByStatusId(Guid statusId)
    {
        logger.LogInformation("Buscando tarefas por status: {StatusId}", statusId);
        return tarefaRepository.GetByStatusId(statusId).Select(TarefaResponse.FromDomain).ToList();
    }

    public TarefaResponse Create(TarefaRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Create");
        activity?.SetTag("tarefa.petId", request.PetId.ToString());
        activity?.SetTag("tarefa.usuarioId", request.UsuarioId.ToString());

        logger.LogInformation("Iniciando criação de tarefa '{Titulo}' para o pet {PetId}, Responsável: {UsuarioId}, Pontos: {PontosTarefa}",
            request.Titulo, request.PetId, request.UsuarioId, request.PontosTarefa);

        if (!petRepository.ExistsById(request.PetId))
        {
            logger.LogWarning("Tentativa de criar tarefa para pet inexistente: {PetId}", request.PetId);
            throw new InvalidOperationException("Pet não encontrado.");
        }

        if (!usuarioRepository.ExistsById(request.UsuarioId))
        {
            logger.LogWarning("Tentativa de atribuir tarefa para usuário inexistente: {UsuarioId}", request.UsuarioId);
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        if (!usuarioPetRepository.Exists(request.UsuarioId, request.PetId))
        {
            logger.LogWarning("Usuário {UsuarioId} não está vinculado ao pet {PetId} como cuidador.", request.UsuarioId, request.PetId);
            throw new InvalidOperationException("Somente cuidadores vinculados ao pet podem receber tarefas.");
        }

        var statusPendente = BuscarStatusObrigatorio("PENDENTE");
        var tarefa = request.ToDomain(statusPendente.Id);
        tarefaRepository.Add(tarefa);
        logger.LogInformation("Tarefa {TarefaId} ('{Titulo}') criada com sucesso com status PENDENTE.", tarefa.Id, tarefa.Titulo);
        return TarefaResponse.FromDomain(tarefa);
    }

    public TarefaResponse? Update(Guid id, TarefaUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização da tarefa: {TarefaId}", id);
        var tarefa = tarefaRepository.GetById(id);
        if (tarefa is null)
        {
            logger.LogWarning("Tentativa de atualizar tarefa inexistente: {TarefaId}", id);
            return null;
        }

        tarefa.Atualizar(request.Titulo, request.PontosTarefa, request.Descricao, request.Prazo);
        tarefaRepository.Update(tarefa);
        logger.LogInformation("Tarefa {TarefaId} atualizada com sucesso.", id);
        return TarefaResponse.FromDomain(tarefa);
    }

    public TarefaResponse Concluir(Guid tarefaId, Guid usuarioId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaService.Concluir");
        activity?.SetTag("tarefa.id", tarefaId.ToString());
        activity?.SetTag("tarefa.usuarioId", usuarioId.ToString());

        logger.LogInformation("Iniciando conclusão da tarefa {TarefaId} pelo usuário {UsuarioId}", tarefaId, usuarioId);
        var tarefa = tarefaRepository.GetById(tarefaId);
        if (tarefa is null)
        {
            logger.LogWarning("Tentativa de concluir tarefa inexistente: {TarefaId}", tarefaId);
            throw new InvalidOperationException("Tarefa não encontrada.");
        }

        if (!usuarioPetRepository.Exists(usuarioId, tarefa.PetId))
        {
            logger.LogWarning("Usuário {UsuarioId} tentou concluir tarefa {TarefaId} sem vínculo com o pet {PetId}",
                usuarioId, tarefaId, tarefa.PetId);
            throw new InvalidOperationException("Somente cuidadores vinculados ao pet podem concluir a tarefa.");
        }

        if (tarefa.Conclusao.HasValue)
        {
            logger.LogWarning("Tentativa de concluir tarefa já concluída anteriormente: {TarefaId}", tarefaId);
            throw new InvalidOperationException("A tarefa já foi concluída.");
        }

        var statusConcluido = BuscarStatusObrigatorio("CONCLUIDO");
        tarefa.AtualizarStatus(statusConcluido.Id);
        tarefa.Concluir();
        tarefaRepository.Update(tarefa);

        historicoRepository.Add(Historico.Registrar("TAREFA_CONCLUIDA", tarefa.PetId));
        logger.LogInformation("Tarefa {TarefaId} concluída com sucesso. Concedidos {Pontos} pontos e registrado histórico TAREFA_CONCLUIDA no pet {PetId}.",
            tarefaId, tarefa.PontosTarefa, tarefa.PetId);

        return TarefaResponse.FromDomain(tarefa);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão da tarefa: {TarefaId}", id);
        var removido = tarefaRepository.Delete(id);
        if (removido)
            logger.LogInformation("Tarefa {TarefaId} excluída com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de tarefa inexistente: {TarefaId}", id);

        return removido;
    }

    private Status BuscarStatusObrigatorio(string nomeStatus)
    {
        var nomeLower = nomeStatus.ToLower();
        var status = statusRepository.FirstOrDefault(s => s.NomeStatus.ToLower() == nomeLower);
        if (status is null)
        {
            logger.LogWarning("Status obrigatório não encontrado no banco: {NomeStatus}", nomeStatus);
            throw new InvalidOperationException($"Status obrigatório não encontrado: {nomeStatus}.");
        }
        return status;
    }
}
