using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Infrastructure.Mongo.Documents;

/// <summary>Documento da coleção "conteudos_aula": conteúdo didático em Markdown de UMA aula (1:1 com AULA).</summary>
[BsonIgnoreExtraElements]
public sealed class ConteudoAulaDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId), BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("aula_id")] public string AulaId { get; set; } = string.Empty;
    [BsonElement("tipo_conteudo")] public string TipoConteudo { get; set; } = string.Empty;
    [BsonElement("corpo_markdown")] public string CorpoMarkdown { get; set; } = string.Empty;
    [BsonElement("links_recursos")] public List<string> LinksRecursos { get; set; } = [];

    [BsonElement("atualizado_em"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; set; }

    public static ConteudoAulaDocument FromRequest(Guid aulaId, ConteudoAulaRequest r) => new()
    {
        AulaId = aulaId.ToString(),
        TipoConteudo = r.TipoConteudo.Trim(),
        CorpoMarkdown = r.CorpoMarkdown,
        LinksRecursos = r.LinksRecursos is null ? [] : [.. r.LinksRecursos],
        AtualizadoEm = DateTime.UtcNow
    };

    public ConteudoAulaResponse ToResponse() =>
        new(Id, Guid.TryParse(AulaId, out var g) ? g : Guid.Empty, TipoConteudo, CorpoMarkdown, LinksRecursos, AtualizadoEm);
}
