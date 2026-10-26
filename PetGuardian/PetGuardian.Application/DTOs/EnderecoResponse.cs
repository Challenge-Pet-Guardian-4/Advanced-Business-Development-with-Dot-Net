using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record EnderecoResponse(
    Guid Id, string Cep, string Rua, string Numero, Guid BairroId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static EnderecoResponse FromDomain(Endereco e) =>
        new(e.Id, e.Cep, e.Rua, e.Numero, e.BairroId);
}
