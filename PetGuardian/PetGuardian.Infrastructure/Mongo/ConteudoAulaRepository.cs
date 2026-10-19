using Microsoft.Extensions.Options;
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
        var chave = aulaId.ToString();
        var doc = await _colecao.Find(d => d.AulaId == chave).FirstOrDefaultAsync(cancellationToken);
        return doc?.ToResponse();
    }

    public async Task<IReadOnlyDictionary<Guid, ConteudoAulaResponse>> GetByAulaIdsAsync(
        IReadOnlyCollection<Guid> aulaIds, CancellationToken cancellationToken = default)
    {
        if (aulaIds.Count == 0)
            return new Dictionary<Guid, ConteudoAulaResponse>();

        var chaves = aulaIds.Select(i => i.ToString()).ToList();
        var docs = await _colecao.Find(Builders<ConteudoAulaDocument>.Filter.In(d => d.AulaId, chaves))
            .ToListAsync(cancellationToken);

        return docs.Select(d => d.ToResponse()).ToDictionary(c => c.AulaId);
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
            d => d.AulaId == doc.AulaId,
            update,
            new FindOneAndUpdateOptions<ConteudoAulaDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

        return atualizado?.ToResponse();
    }

    public async Task<bool> ExcluirPorAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        var chave = aulaId.ToString();
        var resultado = await _colecao.DeleteOneAsync(d => d.AulaId == chave, cancellationToken);
        return resultado.DeletedCount > 0;
    }
}
