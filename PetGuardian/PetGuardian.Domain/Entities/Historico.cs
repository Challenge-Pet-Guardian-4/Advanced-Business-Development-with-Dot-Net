using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Registro histórico de eventos relevantes de um Pet (ex.: tarefa concluída,
/// aula concluída, marco de saúde). Alimenta a linha do tempo do pet.
/// </summary>
public sealed class Historico : BaseEntity
{
    public string   TipoHist { get; private set; } = string.Empty;
    public DateTime DataHist { get; private set; }

    public Guid PetId { get; private set; }
    public Pet? Pet   { get; private set; }

    private Historico() { }

    public Historico(string tipoHist, DateTime dataHist, Guid petId)
    {
        TipoHist = Validar(tipoHist);
        DataHist = dataHist;
        if (petId == Guid.Empty)
            throw new DomainException("O histórico deve estar associado a um pet válido.");
        PetId = petId;
    }

    /// <summary>Atualiza o tipo/data do evento (usado pelo PUT). O pet vinculado não é alterável.</summary>
    public void Atualizar(string tipoHist, DateTime dataHist)
    {
        TipoHist = Validar(tipoHist);
        DataHist = dataHist;
    }

    /// <summary>Cria um registro de histórico "agora", usado por outros serviços (ex.: conclusão de tarefa).</summary>
    public static Historico Registrar(string tipoHist, Guid petId) => new(tipoHist, DateTime.UtcNow, petId);

    private static string Validar(string tipoHist)
    {
        if (string.IsNullOrWhiteSpace(tipoHist))
            throw new DomainException("O tipo do histórico não pode ser vazio.");
        tipoHist = tipoHist.Trim();
        if (tipoHist.Length > 30)
            throw new DomainException("O tipo do histórico deve ter no máximo 30 caracteres.");
        return tipoHist;
    }
}