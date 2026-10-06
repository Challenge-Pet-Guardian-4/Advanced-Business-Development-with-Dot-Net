using System.Text.Json.Serialization;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record HistoricoResponse(
    Guid Id, string TipoHist, DateTime DataHist, Guid PetId,
    [property: JsonPropertyName("_links")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<Link>? Links = null)
{
    public static HistoricoResponse FromDomain(Historico h) => new(h.Id, h.TipoHist, h.DataHist, h.PetId);
}