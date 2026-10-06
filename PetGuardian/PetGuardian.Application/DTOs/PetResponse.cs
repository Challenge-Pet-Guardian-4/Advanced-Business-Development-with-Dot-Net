using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

public record PetResponse(
    Guid Id, string Nome, DateTime DataNascimento, int IdadeEmAnos,
    SexoPet Sexo, PortePet Porte, bool Castrado, Guid RacaId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static PetResponse FromDomain(Pet p) =>
        new(p.Id, p.Nome, p.DataNascimento, p.IdadeEmAnos, p.Sexo, p.Porte, p.Castrado, p.RacaId);
}