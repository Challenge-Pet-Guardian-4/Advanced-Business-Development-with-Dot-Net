using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Entidade de junção N:N entre Usuario e Endereco.
/// Chave composta (UsuarioId, EnderecoId) — não herda BaseEntity.
/// </summary>
public sealed class UsuarioEndereco
{
    public Guid      UsuarioId  { get; private set; }
    public Usuario?  Usuario    { get; private set; }

    public Guid      EnderecoId { get; private set; }
    public Endereco? Endereco   { get; private set; }

    private UsuarioEndereco() { }

    public UsuarioEndereco(Guid usuarioId, Guid enderecoId)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("O vínculo deve ter um usuário válido.");

        if (enderecoId == Guid.Empty)
            throw new DomainException("O vínculo deve ter um endereço válido.");

        UsuarioId  = usuarioId;
        EnderecoId = enderecoId;
    }
}