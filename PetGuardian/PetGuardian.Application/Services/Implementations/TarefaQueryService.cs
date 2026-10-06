using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

public sealed class TarefaQueryService(ITarefaRepository tarefaRepository, ILogger<TarefaQueryService> logger) : ITarefaQueryService
{
    public PagedResponse<TarefaResponse> Search(PageQuery query, TarefaFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TarefaQueryService.Search");
        var q = query.Normalized();
        logger.LogInformation("Pesquisando tarefas: página {Page}, tamanho {PageSize}, ordenação {SortBy} {SortDir}.",
            q.Page, q.PageSize, q.SortBy ?? nameof(Tarefa.Prazo), q.SortDir);

        var titulo = filter.Titulo?.Trim().ToLowerInvariant();
        var filtrarPet = filter.PetId.HasValue;         var petId = filter.PetId.GetValueOrDefault();
        var filtrarUsuario = filter.UsuarioId.HasValue; var usuarioId = filter.UsuarioId.GetValueOrDefault();
        var filtrarStatus = filter.StatusId.HasValue;   var statusId = filter.StatusId.GetValueOrDefault();
        var filtrarConclusao = filter.Concluida.HasValue; var concluida = filter.Concluida.GetValueOrDefault();
        var filtrarDe = filter.PrazoDe.HasValue;        var prazoDe = filter.PrazoDe.GetValueOrDefault();
        var filtrarAte = filter.PrazoAte.HasValue;      var prazoAte = filter.PrazoAte.GetValueOrDefault();

        Expression<Func<Tarefa, bool>> predicate = t =>
            (titulo == null || t.Titulo.ToLower().Contains(titulo))
            && (!filtrarPet || t.PetId == petId)
            && (!filtrarUsuario || t.UsuarioId == usuarioId)
            && (!filtrarStatus || t.StatusId == statusId)
            && (!filtrarConclusao || (concluida && t.Conclusao != null) || (!concluida && t.Conclusao == null))
            && (!filtrarDe || t.Prazo >= prazoDe)
            && (!filtrarAte || t.Prazo <= prazoAte);

        var resultado = tarefaRepository.GetPaged(predicate, q.SortBy ?? nameof(Tarefa.Prazo), q.Descending, q.Page, q.PageSize);
        return PagedResponse<TarefaResponse>.Create(resultado.Map(TarefaResponse.FromDomain), q);
    }
}