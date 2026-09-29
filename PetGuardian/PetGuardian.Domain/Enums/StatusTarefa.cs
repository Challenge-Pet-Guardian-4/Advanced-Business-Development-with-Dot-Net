namespace PetGuardian.Domain.Enums;

/// <summary>
/// Ciclo de vida de uma tarefa associada a um pet.
/// </summary>
public enum StatusTarefa
{
    Pendente = 0,
    EmAndamento = 1,
    Concluida = 2,
    Cancelada = 3
}
