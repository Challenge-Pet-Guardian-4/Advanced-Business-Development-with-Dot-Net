using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record EnderecoResponse(Guid Id, string Cep, string Rua, string Numero, Guid BairroId)
{
    public static EnderecoResponse FromDomain(Endereco e) =>
        new(e.Id, e.Cep, e.Rua, e.Numero, e.BairroId);
}