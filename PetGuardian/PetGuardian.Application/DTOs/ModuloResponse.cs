using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record ModuloResponse(Guid Id, string Nome, string TempoConclusao, string Descricao, Guid TrilhaId)
{
    public static ModuloResponse FromDomain(Modulo m) =>
        new(m.Id, m.Nome, m.TempoConclusao, m.Descricao, m.TrilhaId);
}