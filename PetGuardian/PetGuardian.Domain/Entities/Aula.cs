using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Aula pertencente a um <see cref="Modulo"/>. Concede pontos de gamificação ao ser concluída.
/// O conteúdo didático (Markdown) não é persistido aqui: fica no MongoDB (coleção conteudos_aula).
/// </summary>
public sealed class Aula : BaseEntity
{
    public string Nome        { get; private set; } = string.Empty;
    public string Descricao   { get; private set; } = string.Empty;
    public int    PontosAula  { get; private set; }
    public string Dificuldade { get; private set; } = string.Empty;
    public bool   Concluida   { get; private set; }

    public Guid    ModuloId { get; private set; }
    public Modulo? Modulo   { get; private set; }

    private Aula() { }

    public Aula(string nome, string descricao, int pontosAula, string dificuldade, bool concluida, Guid moduloId)
    {
        if (moduloId == Guid.Empty)
            throw new DomainException("A aula deve estar associada a um módulo válido.");

        Atualizar(nome, descricao, pontosAula, dificuldade, concluida);
        ModuloId = moduloId;
    }

    /// <summary>Atualiza os campos editáveis (usado pelo PUT). O módulo vinculado não é reatribuível por aqui.</summary>
    public void Atualizar(string nome, string descricao, int pontosAula, string dificuldade, bool concluida)
    {
        (Nome, Descricao, PontosAula, Dificuldade) = Validar(nome, descricao, pontosAula, dificuldade);
        Concluida = concluida;
    }

    /// <summary>Marca a aula como concluída.</summary>
    public void Concluir()
    {
        if (Concluida)
            throw new DomainException("Esta aula já foi concluída.");
        Concluida = true;
    }

    /// <summary>Desfaz a conclusão da aula.</summary>
    public void Desmarcar()
    {
        if (!Concluida)
            throw new DomainException("Esta aula não está concluída.");
        Concluida = false;
    }

    private static (string Nome, string Descricao, int PontosAula, string Dificuldade) Validar(
        string nome, string descricao, int pontosAula, string dificuldade)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da aula não pode ser vazio.");
        nome = nome.Trim();
        if (nome.Length > 50)
            throw new DomainException("O nome da aula deve ter no máximo 50 caracteres.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição da aula não pode ser vazia.");
        descricao = descricao.Trim();
        if (descricao.Length > 100)
            throw new DomainException("A descrição da aula deve ter no máximo 100 caracteres.");
        if (pontosAula < 0)
            throw new DomainException("Os pontos da aula não podem ser negativos.");
        if (string.IsNullOrWhiteSpace(dificuldade))
            throw new DomainException("A dificuldade não pode ser vazia.");
        dificuldade = dificuldade.Trim();
        if (dificuldade.Length > 20)
            throw new DomainException("A dificuldade deve ter no máximo 20 caracteres.");
        return (nome, descricao, pontosAula, dificuldade);
    }
}