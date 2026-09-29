using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Raça do pet.</summary>
public sealed class Raca : BaseEntity
{
    public string NomeRaca { get; private set; } = string.Empty;

    public List<Pet> Pets { get; private set; } = [];

    private Raca() { }

    public Raca(string nomeRaca)
    {
        NomeRaca = Validar(nomeRaca);
    }

    /// <summary>Atualiza o nome da raça (usado pelo PUT).</summary>
    public void Atualizar(string nomeRaca) => NomeRaca = Validar(nomeRaca);

    private static string Validar(string nomeRaca)
    {
        if (string.IsNullOrWhiteSpace(nomeRaca))
            throw new DomainException("O nome da raça não pode ser vazio.");
        nomeRaca = nomeRaca.Trim();
        if (nomeRaca.Length > 30)
            throw new DomainException("O nome da raça deve ter no máximo 30 caracteres.");
        return nomeRaca;
    }
}