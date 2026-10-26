using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TelefoneResponse(
    Guid Id, string NumDdd, string NumTel, string Completo,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static TelefoneResponse FromDomain(Telefone t) => new(t.Id, t.NumDdd, t.NumTel, t.Completo);
}
