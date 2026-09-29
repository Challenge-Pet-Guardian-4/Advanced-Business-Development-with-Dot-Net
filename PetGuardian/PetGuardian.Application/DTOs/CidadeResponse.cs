using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record CidadeResponse(Guid Id, string NomeCidade, Guid EstadoId)
{
    public static CidadeResponse FromDomain(Cidade c) => new(c.Id, c.NomeCidade, c.EstadoId);
}