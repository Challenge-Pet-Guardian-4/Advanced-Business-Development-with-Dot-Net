using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record UsuarioEnderecoRequest(
    [Required] Guid UsuarioId,
    [Required] Guid EnderecoId)
{
    public UsuarioEndereco ToDomain() => new(UsuarioId, EnderecoId);
}