using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

public record UsuarioResponse(
    Guid Id, string Nome, string Email, RoleUsuario Role, Guid TelefoneId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static UsuarioResponse FromDomain(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Role, u.TelefoneId);
}