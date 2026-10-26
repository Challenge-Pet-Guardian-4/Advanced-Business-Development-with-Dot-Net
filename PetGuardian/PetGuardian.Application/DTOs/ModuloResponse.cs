using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record ModuloResponse(
    Guid Id, string Nome, string TempoConclusao, string Descricao, Guid TrilhaId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static ModuloResponse FromDomain(Modulo m) =>
        new(m.Id, m.Nome, m.TempoConclusao, m.Descricao, m.TrilhaId);
}