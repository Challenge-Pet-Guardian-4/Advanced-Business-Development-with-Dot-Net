using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record CidadeResponse(
    Guid Id, string NomeCidade, Guid EstadoId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static CidadeResponse FromDomain(Cidade c) => new(c.Id, c.NomeCidade, c.EstadoId);
}
