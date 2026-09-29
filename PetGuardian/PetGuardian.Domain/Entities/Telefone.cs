using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Telefone de contato. Relacionamento 1:1 com <see cref="Usuario"/>.
/// </summary>
public sealed class Telefone : BaseEntity
{
    public string NumDdd { get; private set; } = string.Empty;
    public string NumTel { get; private set; } = string.Empty;

    private Telefone() { }

    public Telefone(string numDdd, string numTel)
    {
        (NumDdd, NumTel) = Validar(numDdd, numTel);
    }

    /// <summary>Atualiza DDD/número (usado pelo PUT).</summary>
    public void Atualizar(string numDdd, string numTel) => (NumDdd, NumTel) = Validar(numDdd, numTel);

    /// <summary>Número completo formatado: (DDD) Número.</summary>
    public string Completo => $"({NumDdd}) {NumTel}";

    private static (string NumDdd, string NumTel) Validar(string numDdd, string numTel)
    {
        if (string.IsNullOrWhiteSpace(numDdd) || numDdd.Trim().Length != 2)
            throw new DomainException("O DDD deve ter exatamente 2 dígitos.");
        if (string.IsNullOrWhiteSpace(numTel) || numTel.Trim().Length > 9)
            throw new DomainException("O número de telefone deve ter no máximo 9 dígitos.");
        return (numDdd.Trim(), numTel.Trim());
    }
}