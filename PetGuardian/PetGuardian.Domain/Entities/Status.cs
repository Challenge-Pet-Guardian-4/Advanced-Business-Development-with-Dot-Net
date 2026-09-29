using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Status de ciclo de vida de uma <see cref="Tarefa"/>.
/// Valores aceitos: CONCLUIDO | EXPIRADO | PENDENTE (reflete o CHECK constraint do banco).
/// </summary>
/// <remarks>Antes também era referenciado por Atendimento (removido).</remarks>
public sealed class Status : BaseEntity
{
    private static readonly string[] ValoresAceitos = ["CONCLUIDO", "EXPIRADO", "PENDENTE"];

    public string NomeStatus { get; private set; } = string.Empty;

    public List<Tarefa> Tarefas { get; private set; } = [];

    private Status() { }

    public Status(string nomeStatus)
    {
        NomeStatus = Validar(nomeStatus);
    }

    /// <summary>Atualiza o nome do status (usado pelo PUT).</summary>
    public void Atualizar(string nomeStatus) => NomeStatus = Validar(nomeStatus);

    private static string Validar(string nomeStatus)
    {
        if (string.IsNullOrWhiteSpace(nomeStatus))
            throw new DomainException("O nome do status não pode ser vazio.");
        nomeStatus = nomeStatus.Trim().ToUpperInvariant();
        if (nomeStatus.Length > 15)
            throw new DomainException("O nome do status deve ter no máximo 15 caracteres.");
        if (!ValoresAceitos.Contains(nomeStatus))
            throw new DomainException("Status inválido. Valores aceitos: CONCLUIDO, EXPIRADO, PENDENTE.");
        return nomeStatus;
    }
}