using System.Text.Json.Serialization;
using PetGuardian.Application.Common;

namespace PetGuardian.Application.DTOs;

/// <summary>
/// Read model NoSQL: trilha com módulos e aulas EMBUTIDOS (mesmo shape do documento
/// da coleção MongoDB "trilhas_educativas").
/// </summary>
public record TrilhaCatalogoResponse(
    string? Id,
    Guid TrilhaIdOrigem,
    string Nome,
    string Descricao,
    PetAlvoCatalogoResponse PetAlvo,
    IReadOnlyList<ModuloCatalogoResponse> Modulos,
    DateTime SincronizadoEm,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null);

public record PetAlvoCatalogoResponse(Guid PetId, string Nome, string Porte);

public record ModuloCatalogoResponse(
    Guid ModuloIdOrigem, string Titulo, string TempoEstimado, string Descricao,
    IReadOnlyList<AulaCatalogoResponse> Aulas);

public record AulaCatalogoResponse(
    Guid AulaIdOrigem, string Titulo, string Descricao, int PontosRecompensa,
    string Dificuldade, bool Concluida, string TipoConteudo, string Conteudo);