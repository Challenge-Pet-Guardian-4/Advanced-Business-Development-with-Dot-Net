using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Infrastructure.Mongo.Documents;

/// <summary>Documento da coleção "trilhas_educativas": módulos e aulas EMBUTIDOS (composição, zero JOIN).</summary>
[BsonIgnoreExtraElements]
public sealed class TrilhaDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId), BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("trilha_id_origem")] public string TrilhaIdOrigem { get; set; } = string.Empty;
    [BsonElement("nome")] public string Nome { get; set; } = string.Empty;
    [BsonElement("descricao")] public string Descricao { get; set; } = string.Empty;
    [BsonElement("pet_alvo")] public PetAlvoDocument PetAlvo { get; set; } = new();
    [BsonElement("modulos")] public List<ModuloDocument> Modulos { get; set; } = [];

    [BsonElement("sincronizado_em"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime SincronizadoEm { get; set; }

    public static TrilhaDocument FromResponse(TrilhaCatalogoResponse r) => new()
    {
        Id = r.Id,
        TrilhaIdOrigem = r.TrilhaIdOrigem.ToString(),
        Nome = r.Nome,
        Descricao = r.Descricao,
        PetAlvo = new PetAlvoDocument { PetId = r.PetAlvo.PetId.ToString(), Nome = r.PetAlvo.Nome, Porte = r.PetAlvo.Porte },
        Modulos = r.Modulos.Select(m => new ModuloDocument
        {
            ModuloId = m.ModuloIdOrigem.ToString(),
            Titulo = m.Titulo,
            TempoEstimado = m.TempoEstimado,
            Descricao = m.Descricao,
            Aulas = m.Aulas.Select(a => new AulaDocument
            {
                AulaId = a.AulaIdOrigem.ToString(),
                Titulo = a.Titulo,
                Descricao = a.Descricao,
                PontosRecompensa = a.PontosRecompensa,
                Dificuldade = a.Dificuldade,
                Concluida = a.Concluida,
                TipoConteudo = a.TipoConteudo,
                Conteudo = a.Conteudo
            }).ToList()
        }).ToList(),
        SincronizadoEm = r.SincronizadoEm
    };

    public TrilhaCatalogoResponse ToResponse() => new(
        Id,
        Guid.Parse(TrilhaIdOrigem),
        Nome,
        Descricao,
        new PetAlvoCatalogoResponse(Guid.Parse(PetAlvo.PetId), PetAlvo.Nome, PetAlvo.Porte),
        Modulos.Select(m => new ModuloCatalogoResponse(
            Guid.Parse(m.ModuloId), m.Titulo, m.TempoEstimado, m.Descricao,
            m.Aulas.Select(a => new AulaCatalogoResponse(
                Guid.Parse(a.AulaId), a.Titulo, a.Descricao, a.PontosRecompensa,
                a.Dificuldade, a.Concluida, a.TipoConteudo, a.Conteudo)).ToList())).ToList(),
        SincronizadoEm);
}

[BsonIgnoreExtraElements]
public sealed class PetAlvoDocument
{
    [BsonElement("pet_id")] public string PetId { get; set; } = string.Empty;
    [BsonElement("nome")] public string Nome { get; set; } = string.Empty;
    [BsonElement("porte")] public string Porte { get; set; } = string.Empty;
}

[BsonIgnoreExtraElements]
public sealed class ModuloDocument
{
    [BsonElement("modulo_id")] public string ModuloId { get; set; } = string.Empty;
    [BsonElement("titulo")] public string Titulo { get; set; } = string.Empty;
    [BsonElement("tempo_estimado")] public string TempoEstimado { get; set; } = string.Empty;
    [BsonElement("descricao")] public string Descricao { get; set; } = string.Empty;
    [BsonElement("aulas")] public List<AulaDocument> Aulas { get; set; } = [];
}

[BsonIgnoreExtraElements]
public sealed class AulaDocument
{
    [BsonElement("aula_id")] public string AulaId { get; set; } = string.Empty;
    [BsonElement("titulo")] public string Titulo { get; set; } = string.Empty;
    [BsonElement("descricao")] public string Descricao { get; set; } = string.Empty;
    [BsonElement("pontos_recompensa")] public int PontosRecompensa { get; set; }
    [BsonElement("dificuldade")] public string Dificuldade { get; set; } = string.Empty;
    [BsonElement("concluida")] public bool Concluida { get; set; }
    [BsonElement("tipo_conteudo")] public string TipoConteudo { get; set; } = string.Empty;
    [BsonElement("conteudo")] public string Conteudo { get; set; } = string.Empty;
}