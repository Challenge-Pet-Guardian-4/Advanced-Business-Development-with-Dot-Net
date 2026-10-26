using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record BairroResponse(
    Guid Id, string NomeBairro, Guid CidadeId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static BairroResponse FromDomain(Bairro b) => new(b.Id, b.NomeBairro, b.CidadeId);
}
