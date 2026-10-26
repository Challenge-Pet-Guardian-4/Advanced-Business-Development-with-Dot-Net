using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>Adicionado para o PUT devolver o recurso atualizado.</summary>
public record StatusResponse(
    Guid Id, string NomeStatus,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static StatusResponse FromDomain(Status s) => new(s.Id, s.NomeStatus);
}
