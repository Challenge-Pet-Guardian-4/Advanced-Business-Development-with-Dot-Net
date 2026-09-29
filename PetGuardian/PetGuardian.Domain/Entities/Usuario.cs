using PetGuardian.Domain.Common;
using PetGuardian.Domain.Enums;
using PetGuardian.Domain.Exceptions;
using PetGuardian.Domain.Helpers;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Usuário do sistema com senha criptografada via BCrypt e Salt. Possui telefone exclusivo (1:1),
/// endereços via N:N (UsuarioEndereco) e relacionamento N:N com Pet via UsuarioPet.
/// </summary>
public sealed class Usuario : BaseEntity
{
    public const int MinimumPasswordLength = 6;

    public string      Nome  { get; private set; } = string.Empty;
    public string      Email { get; private set; } = string.Empty;
    public string      Senha { get; private set; } = string.Empty;
    public string      Salt  { get; private set; } = string.Empty;
    public RoleUsuario Role  { get; private set; }

    public Guid      TelefoneId { get; private set; }
    public Telefone? Telefone   { get; private set; }

    public List<UsuarioEndereco> Enderecos      { get; private set; } = [];
    public List<UsuarioPet>      PetsVinculados { get; private set; } = [];
    public List<Tarefa>          Tarefas        { get; private set; } = [];

    private Usuario() { }

    public Usuario(string nome, string email, string senha, RoleUsuario role, Guid telefoneId)
    {
        AtualizarNome(nome);
        AtualizarEmail(email);
        AtualizarSenha(senha);
        Role = role;
        if (telefoneId == Guid.Empty)
            throw new DomainException("O usuário deve ter um telefone válido.");
        TelefoneId = telefoneId;
    }

    public void AtualizarNome(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome não pode ser vazio.");
        novoNome = novoNome.Trim();
        if (novoNome.Length > 100)
            throw new DomainException("O nome deve ter no máximo 100 caracteres.");
        Nome = novoNome;
    }

    public void AtualizarEmail(string novoEmail)
    {
        if (string.IsNullOrWhiteSpace(novoEmail) || !novoEmail.Contains('@'))
            throw new DomainException("O e-mail informado é inválido.");
        novoEmail = novoEmail.Trim();
        if (novoEmail.Length > 50)
            throw new DomainException("O e-mail deve ter no máximo 50 caracteres.");
        Email = novoEmail;
    }

    public void AtualizarSenha(string novaSenhaRaw)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaRaw) || novaSenhaRaw.Length < MinimumPasswordLength)
            throw new DomainException($"A senha deve ter pelo menos {MinimumPasswordLength} caracteres.");

        Salt = Guid.NewGuid().ToString("N");
        Senha = HashHelper.Hash(novaSenhaRaw, Salt);
    }

    public bool VerifyPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            return false;

        return HashHelper.Verify(rawPassword, Salt, Senha);
    }

    public void AtualizarRole(RoleUsuario novaRole) => Role = novaRole;
}