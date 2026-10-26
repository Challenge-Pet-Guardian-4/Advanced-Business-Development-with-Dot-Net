using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record UsuarioPetResponse(
    Guid UsuarioId, Guid PetId, bool ResponPrinc,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static UsuarioPetResponse FromDomain(UsuarioPet up) => new(up.UsuarioId, up.PetId, up.ResponPrinc);
}
