using PetGuardian.Domain.Common;
using PetGuardian.Domain.Enums;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Pet cadastrado no sistema. Pertence a uma <see cref="Raca"/>.
/// Relacionamento N:N com <see cref="Usuario"/> via <see cref="UsuarioPet"/>.
/// Possui trilhas de cuidado (<see cref="Trilha"/>) e um histórico de eventos (<see cref="Historico"/>).
/// </summary>
/// <remarks>
/// Substitui <c>Idade (int)</c> por <c>DataNascimento (DATE)</c>, conforme novas mudanças no banco de dados.
/// A propriedade <c>Atendimentos</c> foi removida (tabela ATENDIMENTO não existe mais).
/// </remarks>
public sealed class Pet : BaseEntity
{
    public string   Nome           { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public SexoPet  Sexo           { get; private set; }
    public PortePet Porte          { get; private set; }
    public bool     Castrado       { get; private set; }

    public Guid  RacaId { get; private set; }
    public Raca? Raca   { get; private set; }

    // N:N / 1:N
    public List<UsuarioPet> UsuariosPet { get; private set; } = [];
    public List<Tarefa>     Tarefas     { get; private set; } = [];
    public List<Trilha>     Trilhas     { get; private set; } = [];
    public List<Historico>  Historicos  { get; private set; } = [];

    /// <summary>Idade em anos completos, calculada a partir da data de nascimento (não persistida).</summary>
    public int IdadeEmAnos
    {
        get
        {
            var hoje = DateTime.UtcNow.Date;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade))
                idade--;
            return idade;
        }
    }

    private Pet() { }

    public Pet(string nome, DateTime dataNascimento, SexoPet sexo, PortePet porte, bool castrado, Guid racaId)
    {
        Atualizar(nome, dataNascimento, sexo, porte, castrado, racaId);
    }

    /// <summary>Atualiza os dados editáveis do pet (usado pelo endpoint PUT).</summary>
    public void Atualizar(string nome, DateTime dataNascimento, SexoPet sexo, PortePet porte, bool castrado, Guid racaId)
    {
        ValidarNome(nome);
        ValidarDataNascimento(dataNascimento);
        if (racaId == Guid.Empty)
            throw new DomainException("O pet deve estar associado a uma raça válida.");

        Nome           = nome.Trim();
        DataNascimento = dataNascimento.Date;
        Sexo           = sexo;
        Porte          = porte;
        Castrado       = castrado;
        RacaId         = racaId;
    }

    public void Castrar()
    {
        if (Castrado)
            throw new DomainException("O pet já foi castrado.");
        Castrado = true;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do pet não pode ser vazio.");
        if (nome.Trim().Length > 30)
            throw new DomainException("O nome do pet deve ter no máximo 30 caracteres.");
    }

    private static void ValidarDataNascimento(DateTime dataNascimento)
    {
        if (dataNascimento.Date > DateTime.UtcNow.Date)
            throw new DomainException("A data de nascimento não pode estar no futuro.");
        if (dataNascimento.Date < DateTime.UtcNow.Date.AddYears(-40))
            throw new DomainException("A data de nascimento informada é inválida.");
    }
}