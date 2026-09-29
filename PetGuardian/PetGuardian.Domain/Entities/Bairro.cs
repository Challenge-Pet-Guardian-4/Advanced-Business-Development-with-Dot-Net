using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Bairro. Pertence a uma <see cref="Cidade"/>.</summary>
public sealed class Bairro : BaseEntity
{
    public string NomeBairro { get; private set; } = string.Empty;
    public Guid   CidadeId { get; private set; }
    public Cidade? Cidade  { get; private set; }

    public List<Endereco> Enderecos { get; private set; } = [];

    private Bairro() { }

    public Bairro(string nomeBairro, Guid cidadeId)
    {
        (NomeBairro, CidadeId) = Validar(nomeBairro, cidadeId);
    }

    /// <summary>Atualiza os dados do bairro (usado pelo PUT).</summary>
    public void Atualizar(string nomeBairro, Guid cidadeId) => (NomeBairro, CidadeId) = Validar(nomeBairro, cidadeId);

    private static (string NomeBairro, Guid CidadeId) Validar(string nomeBairro, Guid cidadeId)
    {
        if (string.IsNullOrWhiteSpace(nomeBairro))
            throw new DomainException("O nome do bairro não pode ser vazio.");
        nomeBairro = nomeBairro.Trim();
        if (nomeBairro.Length > 30)
            throw new DomainException("O nome do bairro deve ter no máximo 30 caracteres.");
        if (cidadeId == Guid.Empty)
            throw new DomainException("O bairro deve estar associado a uma cidade válida.");
        return (nomeBairro, cidadeId);
    }
}