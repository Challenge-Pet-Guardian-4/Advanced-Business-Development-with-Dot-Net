using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record TrilhaResponse(Guid Id, string Nome, string Descricao, Guid PetId)
{
    public static TrilhaResponse FromDomain(Trilha t) => new(t.Id, t.Nome, t.Descricao, t.PetId);
}