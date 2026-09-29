using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record UsuarioEnderecoResponse(Guid UsuarioId, Guid EnderecoId)
{
    public static UsuarioEnderecoResponse FromDomain(UsuarioEndereco ue) =>
        new(ue.UsuarioId, ue.EnderecoId);
}