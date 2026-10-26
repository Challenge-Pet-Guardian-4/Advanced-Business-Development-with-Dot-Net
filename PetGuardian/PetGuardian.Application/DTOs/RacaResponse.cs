using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record RacaResponse(
    Guid Id, string NomeRaca,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static RacaResponse FromDomain(Raca r) => new(r.Id, r.NomeRaca);
}
