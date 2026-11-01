using System.Globalization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Infrastructure.Mongo.Documents;

namespace PetGuardian.Infrastructure.Mongo;

public sealed class ConteudoAulaRepository : IConteudoAulaRepository
{
    private readonly IMongoCollection<ConteudoAulaDocument> _colecao;

    public ConteudoAulaRepository(IMongoDatabase database, IOptions<MongoDbSettings> options) =>
        _colecao = database.GetCollection<ConteudoAulaDocument>(options.Value.ConteudosAulaCollection);

    public async Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        var filtro = CriarFiltroAulaId(aulaId);
        var doc = await _colecao.Find(filtro).FirstOrDefaultAsync(cancellationToken);
        if (doc is null) return null;
        var resp = doc.ToResponse();
        return resp.AulaId == Guid.Empty ? resp with { AulaId = aulaId } : resp;
    }

    public async Task<IReadOnlyDictionary<Guid, ConteudoAulaResponse>> GetByAulaIdsAsync(
        IReadOnlyCollection<Guid> aulaIds, CancellationToken cancellationToken = default)
    {
        if (aulaIds.Count == 0)
            return new Dictionary<Guid, ConteudoAulaResponse>();

        var chaves = new List<BsonValue>();
        foreach (var id in aulaIds)
        {
            var str = id.ToString();
            chaves.Add(new BsonString(str));
            chaves.Add(new BsonString(str.ToLowerInvariant()));
            chaves.Add(new BsonString(str.ToUpperInvariant()));
            chaves.Add(new BsonString(id.ToString("N")));
            chaves.Add(new BsonString(id.ToString("N").ToLowerInvariant()));

            if (TryExtractInt(id, out var intVal))
            {
                chaves.Add(new BsonInt32(intVal));
                chaves.Add(new BsonInt64(intVal));
                chaves.Add(new BsonString(intVal.ToString(CultureInfo.InvariantCulture)));
            }
        }

        var docs = await _colecao.Find(Builders<ConteudoAulaDocument>.Filter.In("aula_id", chaves))
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<Guid, ConteudoAulaResponse>();
        foreach (var doc in docs)
        {
            var resp = doc.ToResponse();
            var matchingId = aulaIds.FirstOrDefault(id =>
                id == resp.AulaId ||
                string.Equals(id.ToString(), doc.AulaId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id.ToString("N"), doc.AulaId, StringComparison.OrdinalIgnoreCase) ||
                (TryExtractInt(id, out var n) && (doc.AulaId == n.ToString(CultureInfo.InvariantCulture) || doc.AulaId == id.ToString())));

            if (matchingId != Guid.Empty)
                resultado[matchingId] = resp with { AulaId = matchingId };
            else if (resp.AulaId != Guid.Empty)
                resultado[resp.AulaId] = resp;
        }

        return resultado;
    }

    public async Task<ConteudoAulaResponse> InserirAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        var doc = ConteudoAulaDocument.FromRequest(aulaId, request);
        await _colecao.InsertOneAsync(doc, cancellationToken: cancellationToken);
        return doc.ToResponse();
    }

    public async Task<ConteudoAulaResponse?> AtualizarAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        var doc = ConteudoAulaDocument.FromRequest(aulaId, request);
        var update = Builders<ConteudoAulaDocument>.Update
            .Set(d => d.TipoConteudo, doc.TipoConteudo)
            .Set(d => d.CorpoMarkdown, doc.CorpoMarkdown)
            .Set(d => d.LinksRecursos, doc.LinksRecursos)
            .Set(d => d.AtualizadoEm, doc.AtualizadoEm);

        var filtro = CriarFiltroAulaId(aulaId);

        var atualizado = await _colecao.FindOneAndUpdateAsync(
            filtro,
            update,
            new FindOneAndUpdateOptions<ConteudoAulaDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

        if (atualizado is null) return null;
        var resp = atualizado.ToResponse();
        return resp.AulaId == Guid.Empty ? resp with { AulaId = aulaId } : resp;
    }

    public async Task<bool> ExcluirPorAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        var filtro = CriarFiltroAulaId(aulaId);
        var resultado = await _colecao.DeleteOneAsync(filtro, cancellationToken);
        return resultado.DeletedCount > 0;
    }

    private static FilterDefinition<ConteudoAulaDocument> CriarFiltroAulaId(Guid aulaId)
    {
        var chaveString = aulaId.ToString();
        var chaves = new List<BsonValue>
        {
            new BsonString(chaveString),
            new BsonString(chaveString.ToLowerInvariant()),
            new BsonString(chaveString.ToUpperInvariant()),
            new BsonString(aulaId.ToString("N")),
            new BsonString(aulaId.ToString("N").ToLowerInvariant())
        };

        if (TryExtractInt(aulaId, out var intVal))
        {
            chaves.Add(new BsonInt32(intVal));
            chaves.Add(new BsonInt64(intVal));
            chaves.Add(new BsonString(intVal.ToString(CultureInfo.InvariantCulture)));
        }

        return Builders<ConteudoAulaDocument>.Filter.In("aula_id", chaves);
    }

    private static bool TryExtractInt(Guid guid, out int intVal)
    {
        var s = guid.ToString("N");
        // Suporte aos GUIDs sintéticos gerados para IDs numéricos do MongoDB (ex: 00000000000000000000000000000001)
        var trimmed = s.TrimStart('0');
        if (trimmed.Length > 0 && trimmed.Length <= 8 && int.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out intVal))
        {
            return true;
        }

        intVal = 0;
        return false;
    }
}
