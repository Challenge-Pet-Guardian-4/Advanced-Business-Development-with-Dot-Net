using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Persistência poliglota: lê Trilha→Módulo→Aula do relacional (Oracle) em consultas batch
/// e grava cada trilha como UM documento MongoDB, com módulos e aulas embutidos.
/// </summary>
public sealed class TrilhaCatalogoService(
    ITrilhaRepository trilhaRepository,
    IModuloRepository moduloRepository,
    IAulaRepository aulaRepository,
    IPetRepository petRepository,
    ITrilhaCatalogoRepository catalogoRepository,
    ILogger<TrilhaCatalogoService> logger) : ITrilhaCatalogoService
{
    private const string TipoConteudoPadrao = "TEXTO";

    public async Task<SincronizacaoCatalogoResponse> SincronizarAsync(CancellationToken cancellationToken = default)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("TrilhaCatalogoService.Sincronizar");
        logger.LogInformation("Iniciando sincronização do catálogo de trilhas (Oracle -> MongoDB).");

        var trilhas = trilhaRepository.GetAll();
        var modulosPorTrilha = moduloRepository.GetAll().OrderBy(m => m.Nome).ToLookup(m => m.TrilhaId);
        var aulasPorModulo = aulaRepository.GetAll().OrderBy(a => a.Nome).ToLookup(a => a.ModuloId);
        var pets = petRepository.GetAll().ToDictionary(p => p.Id);
        var agora = DateTime.UtcNow;

        var documentos = new List<TrilhaCatalogoResponse>(trilhas.Count);
        foreach (var trilha in trilhas)
        {
            if (!pets.TryGetValue(trilha.PetId, out var pet))
            {
                logger.LogWarning("Trilha {TrilhaId} ignorada: pet {PetId} não encontrado.", trilha.Id, trilha.PetId);
                continue;
            }

            var modulos = modulosPorTrilha[trilha.Id]
                .Select(m => new ModuloCatalogoResponse(
                    m.Id, m.Nome, m.TempoConclusao, m.Descricao,
                    aulasPorModulo[m.Id]
                        .Select(a => new AulaCatalogoResponse(
                            a.Id, a.Nome, a.Descricao, a.PontosAula, a.Dificuldade,
                            a.Concluida, TipoConteudoPadrao, a.Conteudo))
                        .ToList()))
                .ToList();

            documentos.Add(new TrilhaCatalogoResponse(
                null, trilha.Id, trilha.Nome, trilha.Descricao,
                new PetAlvoCatalogoResponse(pet.Id, pet.Nome, pet.Porte.ToString().ToUpperInvariant()),
                modulos, agora));
        }

        var (inseridas, atualizadas) = await catalogoRepository.UpsertAsync(documentos, cancellationToken);
        var removidas = await catalogoRepository.RemoverAusentesAsync(
            documentos.Select(d => d.TrilhaIdOrigem).ToList(), cancellationToken);

        logger.LogInformation("Catálogo sincronizado: {Inseridas} inseridas, {Atualizadas} atualizadas, {Removidas} removidas.",
            inseridas, atualizadas, removidas);

        return new SincronizacaoCatalogoResponse(trilhas.Count, inseridas, atualizadas, removidas, agora);
    }

    public Task<PagedResponse<TrilhaCatalogoResponse>> BuscarAsync(
        PageQuery query, TrilhaCatalogoFilter filter, CancellationToken cancellationToken = default) =>
        BuscarInternoAsync(query.Normalized(), filter, cancellationToken);

    public Task<TrilhaCatalogoResponse?> GetByTrilhaIdAsync(Guid trilhaId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Buscando trilha {TrilhaId} no catálogo MongoDB.", trilhaId);
        return catalogoRepository.GetByTrilhaIdAsync(trilhaId, cancellationToken);
    }

    private async Task<PagedResponse<TrilhaCatalogoResponse>> BuscarInternoAsync(
        PageQuery q, TrilhaCatalogoFilter filter, CancellationToken cancellationToken)
    {
        logger.LogInformation("Pesquisando catálogo MongoDB: página {Page}, tamanho {PageSize}.", q.Page, q.PageSize);
        var resultado = await catalogoRepository.BuscarAsync(filter, q.SortBy, q.Descending, q.Page, q.PageSize, cancellationToken);
        return PagedResponse<TrilhaCatalogoResponse>.Create(resultado, q);
    }
}