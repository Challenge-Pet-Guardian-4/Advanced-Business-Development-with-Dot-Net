using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TrilhaResponse(
    Guid Id, string Nome, string Descricao, Guid PetId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static TrilhaResponse FromDomain(Trilha t) => new(t.Id, t.Nome, t.Descricao, t.PetId);
}