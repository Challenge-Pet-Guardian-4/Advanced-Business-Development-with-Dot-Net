using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record RacaResponse(Guid Id, string NomeRaca)
{
    public static RacaResponse FromDomain(Raca r) => new(r.Id, r.NomeRaca);
}