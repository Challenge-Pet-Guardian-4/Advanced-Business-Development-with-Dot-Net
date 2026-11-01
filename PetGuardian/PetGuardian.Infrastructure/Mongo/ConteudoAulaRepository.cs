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
        var doc = await _colecao.Find(CriarFiltroAulaId(aulaId)).FirstOrDefaultAsync(cancellationToken);
        if (doc is null) return null;
        var resp = doc.ToResponse();
        return resp.AulaId == Guid.Empty ? resp with { AulaId = aulaId } : resp;
    }

    public async Task<IReadOnlyDictionary<Guid, ConteudoAulaResponse>> GetByAulaIdsAsync(
        IReadOnlyCollection<Guid> aulaIds, CancellationToken cancellationToken = default)
    {
        if (aulaIds.Count == 0) return new Dictionary<Guid, ConteudoAulaResponse>();

        var chaves = aulaIds.SelectMany(ObterValoresFiltro).Distinct();
        var docs = await _colecao.Find(Builders<ConteudoAulaDocument>.Filter.In("aula_id", chaves))
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<Guid, ConteudoAulaResponse>();
        foreach (var doc in docs)
        {
            var resp = doc.ToResponse();
            var matchingId = aulaIds.FirstOrDefault(id =>
                resp.AulaId == id ||
                doc.AulaId.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase) ||
                (TryExtractInt(id, out var n) && doc.AulaId == n.ToString(CultureInfo.InvariantCulture)));

            var idEfetivo = matchingId != Guid.Empty ? matchingId : resp.AulaId;
            if (idEfetivo != Guid.Empty)
                resultado[idEfetivo] = resp with { AulaId = idEfetivo };
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

        var atualizado = await _colecao.FindOneAndUpdateAsync(
            CriarFiltroAulaId(aulaId),
            update,
            new FindOneAndUpdateOptions<ConteudoAulaDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

        if (atualizado is null) return null;
        var resp = atualizado.ToResponse();
        return resp.AulaId == Guid.Empty ? resp with { AulaId = aulaId } : resp;
    }

    public async Task<bool> ExcluirPorAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        var resultado = await _colecao.DeleteOneAsync(CriarFiltroAulaId(aulaId), cancellationToken);
        return resultado.DeletedCount > 0;
    }

    private static FilterDefinition<ConteudoAulaDocument> CriarFiltroAulaId(Guid aulaId) =>
        Builders<ConteudoAulaDocument>.Filter.In("aula_id", ObterValoresFiltro(aulaId));

    private static IEnumerable<BsonValue> ObterValoresFiltro(Guid id)
    {
        yield return new BsonString(id.ToString());
        if (TryExtractInt(id, out var n))
        {
            yield return new BsonInt32(n);
            yield return new BsonInt64(n);
            yield return new BsonString(n.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static bool TryExtractInt(Guid guid, out int intVal)
    {
        intVal = 0;
        var s = guid.ToString("N").TrimStart('0');
        return s.Length is > 0 and <= 8 && int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out intVal);
    }
}
