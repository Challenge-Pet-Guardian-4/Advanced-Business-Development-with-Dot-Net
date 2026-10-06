using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Infrastructure.Mongo.Documents;

namespace PetGuardian.Infrastructure.Mongo;

public sealed class TrilhaCatalogoRepository : ITrilhaCatalogoRepository
{
    // whitelist de ordenação (nome público -> campo do documento)
    private static readonly Dictionary<string, string> CamposOrdenaveis = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nome"] = "nome",
        ["descricao"] = "descricao",
        ["sincronizadoEm"] = "sincronizado_em",
        ["petNome"] = "pet_alvo.nome"
    };

    private readonly IMongoCollection<TrilhaDocument> _colecao;

    public TrilhaCatalogoRepository(IMongoDatabase database, IOptions<MongoDbSettings> options) =>
        _colecao = database.GetCollection<TrilhaDocument>(options.Value.TrilhasCollection);

    public async Task<(int Inseridas, int Atualizadas)> UpsertAsync(
        IReadOnlyCollection<TrilhaCatalogoResponse> trilhas, CancellationToken cancellationToken = default)
    {
        if (trilhas.Count == 0)
            return (0, 0);

        var modelos = trilhas.Select(t =>
        {
            var doc = TrilhaDocument.FromResponse(t);
            return new ReplaceOneModel<TrilhaDocument>(
                Builders<TrilhaDocument>.Filter.Eq(d => d.TrilhaIdOrigem, doc.TrilhaIdOrigem), doc)
            { IsUpsert = true };
        }).ToList();

        var resultado = await _colecao.BulkWriteAsync(modelos, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
        return (resultado.Upserts.Count, (int)resultado.MatchedCount);
    }

    public async Task<long> RemoverAusentesAsync(
        IReadOnlyCollection<Guid> trilhaIdsPresentes, CancellationToken cancellationToken = default)
    {
        var chaves = trilhaIdsPresentes.Select(i => i.ToString()).ToList();
        var resultado = await _colecao.DeleteManyAsync(
            Builders<TrilhaDocument>.Filter.Nin(d => d.TrilhaIdOrigem, chaves), cancellationToken);
        return resultado.DeletedCount;
    }

    public async Task<TrilhaCatalogoResponse?> GetByTrilhaIdAsync(Guid trilhaId, CancellationToken cancellationToken = default)
    {
        var chave = trilhaId.ToString();
        var doc = await _colecao.Find(d => d.TrilhaIdOrigem == chave).FirstOrDefaultAsync(cancellationToken);
        return doc?.ToResponse();
    }

    public async Task<PagedResult<TrilhaCatalogoResponse>> BuscarAsync(
        TrilhaCatalogoFilter filter, string? sortBy, bool descending, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var fb = Builders<TrilhaDocument>.Filter;
        var filtros = new List<FilterDefinition<TrilhaDocument>>();

        if (!string.IsNullOrWhiteSpace(filter.Termo))
        {
            var regex = new BsonRegularExpression(Regex.Escape(filter.Termo.Trim()), "i");
            filtros.Add(fb.Or(fb.Regex(d => d.Nome, regex), fb.Regex(d => d.Descricao, regex)));
        }

        if (filter.PetId.HasValue)
            filtros.Add(fb.Eq("pet_alvo.pet_id", filter.PetId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(filter.Dificuldade))
            filtros.Add(fb.Eq("modulos.aulas.dificuldade", filter.Dificuldade.Trim()));

        var filtro = filtros.Count == 0 ? fb.Empty : fb.And(filtros);

        var campo = ResolverCampo(sortBy);
        var sort = Builders<TrilhaDocument>.Sort;
        var ordenacao = sort.Combine(
            descending ? sort.Descending(campo) : sort.Ascending(campo),
            sort.Ascending("_id"));

        var total = await _colecao.CountDocumentsAsync(filtro, cancellationToken: cancellationToken);
        var docs = await _colecao.Find(filtro)
            .Sort(ordenacao)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TrilhaCatalogoResponse>(docs.Select(d => d.ToResponse()).ToList(), (int)total);
    }

    private static string ResolverCampo(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return CamposOrdenaveis["nome"];

        return CamposOrdenaveis.TryGetValue(sortBy.Trim(), out var campo)
            ? campo
            : throw new ArgumentException(
                $"Campo de ordenação inválido: '{sortBy}'. Campos permitidos: {string.Join(", ", CamposOrdenaveis.Keys)}.",
                nameof(sortBy));
    }
}