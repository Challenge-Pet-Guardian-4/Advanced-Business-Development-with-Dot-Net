using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Trilha de cuidado/aprendizado vinculada a um Pet. Agrupa <see cref="Modulo"/>s.</summary>
public sealed class Trilha : BaseEntity
{
    public string Nome      { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    public Guid PetId { get; private set; }
    public Pet? Pet   { get; private set; }

    public List<Modulo> Modulos { get; private set; } = [];

    private Trilha() { }

    public Trilha(string nome, string descricao, Guid petId)
    {
        if (petId == Guid.Empty)
            throw new DomainException("A trilha deve estar associada a um pet válido.");
        Atualizar(nome, descricao);
        PetId = petId;
    }

    /// <summary>Atualiza nome e descrição (usado pelo PUT). O pet vinculado não é alterável.</summary>
    public void Atualizar(string nome, string descricao) => (Nome, Descricao) = Validar(nome, descricao);

    private static (string Nome, string Descricao) Validar(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da trilha não pode ser vazio.");
        nome = nome.Trim();
        if (nome.Length > 30)
            throw new DomainException("O nome da trilha deve ter no máximo 30 caracteres.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição da trilha não pode ser vazia.");
        descricao = descricao.Trim();
        if (descricao.Length > 200)
            throw new DomainException("A descrição da trilha deve ter no máximo 200 caracteres.");
        return (nome, descricao);
    }
}