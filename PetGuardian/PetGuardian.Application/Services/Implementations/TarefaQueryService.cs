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
}