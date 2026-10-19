using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PetGuardian.Application.Common;

namespace PetGuardian.Application.DTOs;

/// <summary>Conteúdo didático de uma aula (documento da coleção MongoDB "conteudos_aula").</summary>
public record ConteudoAulaRequest(
    [Required][StringLength(30, MinimumLength = 2)] string TipoConteudo,
    [Required][MinLength(1)] string CorpoMarkdown,
    List<string>? LinksRecursos);

public record ConteudoAulaResponse(
    string? Id,
    Guid AulaId,
    string TipoConteudo,
    string CorpoMarkdown,
    IReadOnlyList<string> LinksRecursos,
    DateTime AtualizadoEm,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null);
