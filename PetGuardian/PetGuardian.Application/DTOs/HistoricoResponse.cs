using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record HistoricoResponse(Guid Id, string TipoHist, DateTime DataHist, Guid PetId)
{
    public static HistoricoResponse FromDomain(Historico h) => new(h.Id, h.TipoHist, h.DataHist, h.PetId);
}