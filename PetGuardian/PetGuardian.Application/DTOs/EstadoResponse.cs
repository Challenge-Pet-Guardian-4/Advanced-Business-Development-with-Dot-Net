using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record EstadoResponse(Guid Id, string NomeEstado)
{
    public static EstadoResponse FromDomain(Estado e) => new(e.Id, e.NomeEstado);
}