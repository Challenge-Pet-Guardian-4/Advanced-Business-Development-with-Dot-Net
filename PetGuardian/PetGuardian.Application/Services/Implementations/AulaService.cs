using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
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
    public PagedResponse<AulaResponse> Search(PageQuery query, AulaFilter filter)
    {
        var q = query.Normalized();
        var nome = filter.Nome?.Trim().ToLowerInvariant();
        var dificuldade = filter.Dificuldade?.Trim().ToLowerInvariant();
        Expression<Func<Aula, bool>> predicate = a =>
            (nome == null || a.Nome.ToLower().Contains(nome))
            && (filter.ModuloId == null || a.ModuloId == filter.ModuloId)
            && (dificuldade == null || a.Dificuldade.ToLower() == dificuldade)
            && (filter.Concluida == null || a.Concluida == filter.Concluida);

        var resultado = aulaRepository.GetPaged(predicate, q.SortBy ?? nameof(Aula.Nome), q.Descending, q.Page, q.PageSize);
        var conteudos = BuscarConteudosEmLote(resultado.Items);
        return PagedResponse<AulaResponse>.Create(
            resultado.Map(a => AulaResponse.FromDomain(a, conteudos.GetValueOrDefault(a.Id))), q);
    }

    public AulaResponse? GetById(Guid id) =>
        aulaRepository.GetById(id) is { } aula ? AulaResponse.FromDomain(aula, BuscarConteudoMarkdown(id)) : null;

    public IReadOnlyList<AulaResponse> GetByModuloId(Guid moduloId)
    {
        var aulas = aulaRepository.GetByModuloId(moduloId);
        var conteudos = BuscarConteudosEmLote(aulas);
        return aulas.Select(a => AulaResponse.FromDomain(a, conteudos.GetValueOrDefault(a.Id))).ToList();
    }

    public AulaResponse Create(AulaRequest request)
    {
        if (!moduloRepository.ExistsById(request.ModuloId))
            throw new InvalidOperationException("Módulo não encontrado.");

        var aula = aulaRepository.Add(request.ToDomain());
        logger.LogInformation("Aula {AulaId} ('{Nome}') cadastrada no módulo {ModuloId}.", aula.Id, aula.Nome, aula.ModuloId);
        return AulaResponse.FromDomain(aula);
    }

    public AulaResponse? Update(Guid id, AulaUpdateRequest request) =>
        AlterarAula(id, a => a.Atualizar(request.Nome, request.Descricao, request.PontosAula, request.Dificuldade, request.Concluida));

    public AulaResponse? Concluir(Guid id) => AlterarAula(id, a => a.Concluir());

    public AulaResponse? Desmarcar(Guid id) => AlterarAula(id, a => a.Desmarcar());

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!aulaRepository.Delete(id))
            return false;

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

    private AulaResponse? AlterarAula(Guid id, Action<Aula> mutacao)
    {
        var aula = aulaRepository.GetById(id);
        if (aula is null)
            return null;

        mutacao(aula);
        aulaRepository.Update(aula);
        return AulaResponse.FromDomain(aula, BuscarConteudoMarkdown(id));
    }

    private string? BuscarConteudoMarkdown(Guid aulaId) =>
        ExecutarConsultaMongo(async () => (await conteudoRepository.GetByAulaIdAsync(aulaId))?.CorpoMarkdown, null);

    private IReadOnlyDictionary<Guid, string> BuscarConteudosEmLote(IReadOnlyCollection<Aula> aulas) =>
        aulas.Count == 0
            ? new Dictionary<Guid, string>()
            : ExecutarConsultaMongo<IReadOnlyDictionary<Guid, string>>(
                async () => (await conteudoRepository.GetByAulaIdsAsync(aulas.Select(a => a.Id).ToList()))
                    .ToDictionary(kv => kv.Key, kv => kv.Value.CorpoMarkdown),
                new Dictionary<Guid, string>());

    private T ExecutarConsultaMongo<T>(Func<Task<T>> consulta, T valorPadrao)
    {
        try
        {
            return Task.Run(consulta).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar conteúdo complementar no MongoDB.");
            return valorPadrao;
        }
    }
}
