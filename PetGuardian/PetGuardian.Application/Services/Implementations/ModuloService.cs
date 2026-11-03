using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de módulos de aprendizagem vinculados a trilhas.
/// </summary>
public sealed class ModuloService(
    IModuloRepository      moduloRepository,
    ITrilhaRepository      trilhaRepository,
    ILogger<ModuloService> logger) : IModuloService
{
    public PagedResponse<ModuloResponse> Search(PageQuery query, ModuloFilter filter)
    {
        var q = query.Normalized();
        var nome = filter.Nome?.Trim().ToLowerInvariant();
        Expression<Func<Modulo, bool>> predicate = m =>
            (nome == null || m.Nome.ToLower().Contains(nome))
            && (filter.TrilhaId == null || m.TrilhaId == filter.TrilhaId);

        var resultado = moduloRepository.GetPaged(predicate, q.SortBy ?? nameof(Modulo.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<ModuloResponse>.Create(resultado.Map(ModuloResponse.FromDomain), q);
    }

    public ModuloResponse? GetById(Guid id) =>
        moduloRepository.GetById(id) is { } modulo ? ModuloResponse.FromDomain(modulo) : null;

    public IReadOnlyList<ModuloResponse> GetByTrilhaId(Guid trilhaId) =>
        moduloRepository.GetByTrilhaId(trilhaId).Select(ModuloResponse.FromDomain).ToList();

    public ModuloResponse Create(ModuloRequest request)
    {
        if (!trilhaRepository.ExistsById(request.TrilhaId))
            throw new InvalidOperationException("Trilha não encontrada.");

        var modulo = moduloRepository.Add(request.ToDomain());
        logger.LogInformation("Módulo {ModuloId} ('{Nome}') cadastrado na trilha {TrilhaId}.", modulo.Id, modulo.Nome, modulo.TrilhaId);
        return ModuloResponse.FromDomain(modulo);
    }

    public ModuloResponse? Update(Guid id, ModuloUpdateRequest request)
    {
        var modulo = moduloRepository.GetById(id);
        if (modulo is null)
            return null;

        modulo.Atualizar(request.Nome, request.TempoConclusao, request.Descricao);
        moduloRepository.Update(modulo);
        return ModuloResponse.FromDomain(modulo);
    }

    public bool Delete(Guid id) => moduloRepository.Delete(id);
}
