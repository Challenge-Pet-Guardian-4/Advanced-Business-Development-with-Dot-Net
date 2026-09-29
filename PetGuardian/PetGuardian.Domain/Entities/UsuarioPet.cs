using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Entidade de junção N:N entre <see cref="Usuario"/> e <see cref="Pet"/>.
/// Além da ligação, indica se o usuário é o responsável principal pelo pet.
/// Chave composta: (UsuarioId, PetId) — não herda <c>BaseEntity</c>.
/// </summary>
public sealed class UsuarioPet
{
    public Guid     UsuarioId    { get; private set; }
    public Usuario? Usuario      { get; private set; }

    public Guid  PetId { get; private set; }
    public Pet?  Pet   { get; private set; }

    /// <summary>Indica se este usuário é o responsável principal pelo pet.</summary>
    public bool ResponPrinc { get; private set; }

    private UsuarioPet() { }

    public UsuarioPet(Guid usuarioId, Guid petId, bool responPrinc)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("O vínculo deve ter um usuário válido.");

        if (petId == Guid.Empty)
            throw new DomainException("O vínculo deve ter um pet válido.");

        UsuarioId   = usuarioId;
        PetId       = petId;
        ResponPrinc = responPrinc;
    }

    /// <summary>Alterna a responsabilidade principal.</summary>
    public void AtualizarResponsabilidade(bool responsavel) => ResponPrinc = responsavel;
}