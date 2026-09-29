using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>Adicionado para o PUT devolver o recurso atualizado.</summary>
public record StatusResponse(Guid Id, string NomeStatus)
{
    public static StatusResponse FromDomain(Status s) => new(s.Id, s.NomeStatus);
}