using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TelefoneResponse(Guid Id, string NumDdd, string NumTel, string Completo)
{
    public static TelefoneResponse FromDomain(Telefone t) => new(t.Id, t.NumDdd, t.NumTel, t.Completo);
}