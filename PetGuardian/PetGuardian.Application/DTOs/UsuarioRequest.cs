using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Cadastro de usuário com telefone (DDD + número) e endereço (CEP + número) inline, alinhado ao Java.</summary>
public record UsuarioRequest(
    [Required][StringLength(100, MinimumLength = 2)] string Nome,
    [Required][EmailAddress][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6)] string Senha,
    [Required] RoleUsuario Role,
    [Required][StringLength(2, MinimumLength = 2, ErrorMessage = "O DDD deve ter 2 dígitos")] string Ddd,
    [Required][StringLength(9, MinimumLength = 8, ErrorMessage = "O número deve ter entre 8 e 9 dígitos")] string NumeroTelefone,
    [Required] EnderecoRequest Endereco)
{
    public Telefone ToTelefone() => new(Ddd, NumeroTelefone);
    public Usuario ToDomain(Guid telefoneId, Guid enderecoId) => new(Nome, Email, Senha, Role, telefoneId, enderecoId);
}