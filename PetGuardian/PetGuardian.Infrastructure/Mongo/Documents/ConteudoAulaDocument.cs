using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Infrastructure.Mongo.Documents;

/// <summary>
/// Serializador flexível para <c>aula_id</c>: aceita BsonType Int32, Int64, Double, String e ObjectId na leitura,
/// e serializa inteiros numéricos como Int32 e GUIDs/strings como String.
/// </summary>
public sealed class FlexibleAulaIdSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.CurrentBsonType;
        return bsonType switch
        {
            BsonType.Int32 => context.Reader.ReadInt32().ToString(CultureInfo.InvariantCulture),
            BsonType.Int64 => context.Reader.ReadInt64().ToString(CultureInfo.InvariantCulture),
            BsonType.Double => ((long)context.Reader.ReadDouble()).ToString(CultureInfo.InvariantCulture),
            BsonType.String => context.Reader.ReadString(),
            BsonType.ObjectId => context.Reader.ReadObjectId().ToString(),
            BsonType.Null => ReadNull(context),
            _ => ReadFallback(context)
        };
    }

    private static string ReadNull(BsonDeserializationContext context)
    {
        context.Reader.ReadNull();
        return string.Empty;
    }

    private static string ReadFallback(BsonDeserializationContext context)
    {
        context.Reader.SkipValue();
        return string.Empty;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal))
            context.Writer.WriteInt32(intVal);
        else if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longVal))
            context.Writer.WriteInt64(longVal);
        else
            context.Writer.WriteString(value ?? string.Empty);
    }
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
        LinksRecursos = r.LinksRecursos is null ? [] : [.. r.LinksRecursos],
        AtualizadoEm = DateTime.UtcNow
    };

    public ConteudoAulaResponse ToResponse()
    {
        Guid guid;
        if (Guid.TryParse(AulaId, out var parsedGuid))
        {
            guid = parsedGuid;
        }
        else if (int.TryParse(AulaId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
        {
            guid = Guid.Parse($"00000000-0000-0000-0000-{intId:x12}");
        }
        else
        {
            guid = Guid.Empty;
        }

        return new ConteudoAulaResponse(Id, guid, TipoConteudo, CorpoMarkdown, LinksRecursos, AtualizadoEm);
    }
}
