using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Aula pertencente a um <see cref="Modulo"/>. Concede pontos de gamificação ao ser concluída.
/// </summary>
public sealed class Aula : BaseEntity
{
    public string Nome        { get; private set; } = string.Empty;
    public string Descricao   { get; private set; } = string.Empty;
    public int    PontosAula  { get; private set; }
    public string Dificuldade { get; private set; } = string.Empty;
    public string Conteudo    { get; private set; } = string.Empty;
    public bool   Concluida   { get; private set; }

    public Guid    ModuloId { get; private set; }
    public Modulo? Modulo   { get; private set; }

    private Aula() { }

    public Aula(string nome, string descricao, int pontosAula, string dificuldade, string conteudo, bool concluida, Guid moduloId)
    {
        if (moduloId == Guid.Empty)
            throw new DomainException("A aula deve estar associada a um módulo válido.");

        Atualizar(nome, descricao, pontosAula, dificuldade, conteudo, concluida);
        ModuloId = moduloId;
    }

    /// <summary>Atualiza os campos editáveis (usado pelo PUT). O módulo vinculado não é reatribuível por aqui.</summary>
    public void Atualizar(string nome, string descricao, int pontosAula, string dificuldade, string conteudo, bool concluida)
    {
        (Nome, Descricao, PontosAula, Dificuldade, Conteudo) = Validar(nome, descricao, pontosAula, dificuldade, conteudo);
        Concluida = concluida;
    }

    /// <summary>Marca a aula como concluída (idempotente para trás: não permite "desconcluir" por aqui).</summary>
    public void Concluir()
    {
        if (Concluida)
            throw new DomainException("Esta aula já foi concluída.");
        Concluida = true;
    }

    private static (string Nome, string Descricao, int PontosAula, string Dificuldade, string Conteudo) Validar(
        string nome, string descricao, int pontosAula, string dificuldade, string conteudo)
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
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new DomainException("O conteúdo da aula não pode ser vazio.");
        conteudo = conteudo.Trim();
        if (conteudo.Length > 1000)
            throw new DomainException("O conteúdo da aula deve ter no máximo 1000 caracteres.");
        return (nome, descricao, pontosAula, dificuldade, conteudo);
    }
}