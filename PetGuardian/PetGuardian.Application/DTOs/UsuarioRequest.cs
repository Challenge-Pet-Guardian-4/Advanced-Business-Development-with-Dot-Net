using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.DTOs;

/// <summary>Cadastro de usuário com telefone (DDD + número) e endereço (CEP + número) inline.</summary>
public record UsuarioRequest(
    [Required][StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")] string Nome,
    [Required][EmailAddress(ErrorMessage = "E-mail com formato inválido.")][StringLength(50)] string Email,
    [Required][StringLength(60, MinimumLength = 6, ErrorMessage = "A senha deve conter no mínimo 6 caracteres.")] string Senha,
    [Required] RoleUsuario Role,
    [Required][RegularExpression(@"^\d{2}$", ErrorMessage = "O DDD deve conter exatamente 2 dígitos numéricos.")] string Ddd,
    [Required][RegularExpression(@"^\d{8,9}$", ErrorMessage = "O número deve conter entre 8 e 9 dígitos numéricos.")] string NumeroTelefone,
    [Required] EnderecoRequest Endereco)
{
    public Telefone ToTelefone() => new(Ddd, NumeroTelefone);
    public Usuario ToDomain(Guid telefoneId, Guid enderecoId) => new(Nome, Email, Senha, Role, telefoneId, enderecoId);
}
