using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Cidade. Pertence a um <see cref="Estado"/>.</summary>
public sealed class Cidade : BaseEntity
{
    public string NomeCidade { get; private set; } = string.Empty;
    public Guid    EstadoId { get; private set; }
    public Estado? Estado   { get; private set; }

    public List<Bairro> Bairros { get; private set; } = [];

    private Cidade() { }

    public Cidade(string nomeCidade, Guid estadoId)
    {
        (NomeCidade, EstadoId) = Validar(nomeCidade, estadoId);
    }

    /// <summary>Atualiza os dados da cidade (usado pelo PUT).</summary>
    public void Atualizar(string nomeCidade, Guid estadoId) => (NomeCidade, EstadoId) = Validar(nomeCidade, estadoId);

    private static (string NomeCidade, Guid EstadoId) Validar(string nomeCidade, Guid estadoId)
    {
        if (string.IsNullOrWhiteSpace(nomeCidade))
            throw new DomainException("O nome da cidade não pode ser vazio.");
        nomeCidade = nomeCidade.Trim();
        if (nomeCidade.Length > 30)
            throw new DomainException("O nome da cidade deve ter no máximo 30 caracteres.");
        if (estadoId == Guid.Empty)
            throw new DomainException("A cidade deve estar associada a um estado válido.");
        return (nomeCidade, estadoId);
    }
}