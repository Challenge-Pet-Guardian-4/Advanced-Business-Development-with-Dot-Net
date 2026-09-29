using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Módulo de uma <see cref="Trilha"/>. Agrupa <see cref="Aula"/>s.</summary>
public sealed class Modulo : BaseEntity
{
    public string Nome           { get; private set; } = string.Empty;
    public string TempoConclusao { get; private set; } = string.Empty;
    public string Descricao      { get; private set; } = string.Empty;

    public Guid    TrilhaId { get; private set; }
    public Trilha? Trilha   { get; private set; }

    public List<Aula> Aulas { get; private set; } = [];

    private Modulo() { }

    public Modulo(string nome, string tempoConclusao, string descricao, Guid trilhaId)
    {
        if (trilhaId == Guid.Empty)
            throw new DomainException("O módulo deve estar associado a uma trilha válida.");
        Atualizar(nome, tempoConclusao, descricao);
        TrilhaId = trilhaId;
    }

    /// <summary>Atualiza os campos editáveis (usado pelo PUT). A trilha vinculada não é alterável.</summary>
    public void Atualizar(string nome, string tempoConclusao, string descricao) =>
        (Nome, TempoConclusao, Descricao) = Validar(nome, tempoConclusao, descricao);

    private static (string Nome, string TempoConclusao, string Descricao) Validar(
        string nome, string tempoConclusao, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do módulo não pode ser vazio.");
        nome = nome.Trim();
        if (nome.Length > 50)
            throw new DomainException("O nome do módulo deve ter no máximo 50 caracteres.");
        if (string.IsNullOrWhiteSpace(tempoConclusao))
            throw new DomainException("O tempo de conclusão não pode ser vazio.");
        tempoConclusao = tempoConclusao.Trim();
        if (tempoConclusao.Length > 10)
            throw new DomainException("O tempo de conclusão deve ter no máximo 10 caracteres.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição do módulo não pode ser vazia.");
        descricao = descricao.Trim();
        if (descricao.Length > 100)
            throw new DomainException("A descrição do módulo deve ter no máximo 100 caracteres.");
        return (nome, tempoConclusao, descricao);
    }
}