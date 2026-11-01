using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Infrastructure.Mongo.Documents;

/// <summary>
/// Serializador flexível para <c>aula_id</c>: aceita BsonType String, Int32 e Int64 na leitura,
/// e persiste como String padrão na escrita.
/// </summary>
public sealed class FlexibleAulaIdSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        switch (context.Reader.CurrentBsonType)
        {
            case BsonType.String: return context.Reader.ReadString();
            case BsonType.Int32:  return context.Reader.ReadInt32().ToString(CultureInfo.InvariantCulture);
            case BsonType.Int64:  return context.Reader.ReadInt64().ToString(CultureInfo.InvariantCulture);
            case BsonType.Null:   context.Reader.ReadNull(); return string.Empty;
            default:              context.Reader.SkipValue(); return string.Empty;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value) =>
        context.Writer.WriteString(value ?? string.Empty);
}

/// <summary>Documento da coleção "conteudos_aula": conteúdo didático em Markdown de UMA aula (1:1 com AULA).</summary>
[BsonIgnoreExtraElements]
public sealed class ConteudoAulaDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId), BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("aula_id"), BsonSerializer(typeof(FlexibleAulaIdSerializer))]
    public string AulaId { get; set; } = string.Empty;
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
        LinksRecursos = r.LinksRecursos ?? [],
        AtualizadoEm = DateTime.UtcNow
    };

    public ConteudoAulaResponse ToResponse()
    {
        var guid = Guid.TryParse(AulaId, out var parsed) ? parsed
            : int.TryParse(AulaId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId)
                ? Guid.Parse($"00000000-0000-0000-0000-{intId:x12}")
                : Guid.Empty;

        return new ConteudoAulaResponse(Id, guid, TipoConteudo, CorpoMarkdown, LinksRecursos, AtualizadoEm);
    }
}
