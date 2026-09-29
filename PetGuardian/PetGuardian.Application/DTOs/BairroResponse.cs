using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record BairroResponse(Guid Id, string NomeBairro, Guid CidadeId)
{
    public static BairroResponse FromDomain(Bairro b) => new(b.Id, b.NomeBairro, b.CidadeId);
}