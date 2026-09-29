using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>
/// Tarefa de cuidado vinculada a um Pet e a um Usuario responsável.
/// </summary>
/// <remarks>
/// <c>VeterinarioId</c> foi removido (tabela VETERINARIO não existe mais) e
/// <c>UsuarioId</c> deixou de ser opcional — toda tarefa agora nasce com um responsável.
/// </remarks>
public sealed class Tarefa : BaseEntity
{
    public string    Titulo       { get; private set; } = string.Empty;
    public int       PontosTarefa { get; private set; }
    public string    Descricao    { get; private set; } = string.Empty;
    public DateTime  Criacao      { get; private set; }
    public DateTime  Prazo        { get; private set; }
    public DateTime? Conclusao    { get; private set; }

    public Guid     UsuarioId { get; private set; }
    public Usuario? Usuario   { get; private set; }

    public Guid PetId { get; private set; }
    public Pet? Pet   { get; private set; }

    public Guid    StatusId { get; private set; }
    public Status? Status   { get; private set; }

    private Tarefa() { }

    public Tarefa(
        string   titulo,
        int      pontosTarefa,
        string   descricao,
        DateTime prazo,
        Guid     petId,
        Guid     statusId,
        Guid     usuarioId)
    {
        if (petId == Guid.Empty)
            throw new DomainException("A tarefa deve estar associada a um pet válido.");
        if (statusId == Guid.Empty)
            throw new DomainException("O status da tarefa é obrigatório.");
        if (usuarioId == Guid.Empty)
            throw new DomainException("A tarefa deve estar associada a um usuário responsável válido.");

        Atualizar(titulo, pontosTarefa, descricao, prazo);
        Criacao   = DateTime.UtcNow;
        PetId     = petId;
        StatusId  = statusId;
        UsuarioId = usuarioId;
    }

    /// <summary>Atualiza os campos editáveis de uma tarefa ainda não concluída (usado pelo PUT).</summary>
    public void Atualizar(string titulo, int pontosTarefa, string descricao, DateTime prazo)
    {
        if (Conclusao is not null)
            throw new DomainException("Não é possível editar uma tarefa já concluída.");

        ValidarCamposEditaveis(titulo, pontosTarefa, descricao, prazo);

        Titulo       = titulo.Trim();
        PontosTarefa = pontosTarefa;
        Descricao    = descricao.Trim();
        Prazo        = prazo;
    }

    public void AtualizarStatus(Guid statusId)
    {
        if (statusId == Guid.Empty)
            throw new DomainException("Id do status inválido.");
        StatusId = statusId;
    }

    public void Concluir()
    {
        if (Conclusao is not null)
            throw new DomainException("A tarefa já foi concluída.");
        Conclusao = DateTime.UtcNow;
    }

    public void EstenderPrazo(DateTime novoPrazo)
    {
        if (novoPrazo <= DateTime.UtcNow)
            throw new DomainException("O novo prazo deve ser futuro.");
        if (novoPrazo <= Prazo)
            throw new DomainException("O novo prazo deve ser posterior ao prazo atual.");
        Prazo = novoPrazo;
    }

    private static void ValidarCamposEditaveis(string titulo, int pontosTarefa, string descricao, DateTime prazo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("O título não pode ser vazio.");
        if (titulo.Trim().Length > 30)
            throw new DomainException("O título deve ter no máximo 30 caracteres.");
        if (pontosTarefa < 0)
            throw new DomainException("Os pontos não podem ser negativos.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição não pode ser vazia.");
        if (descricao.Trim().Length > 200)
            throw new DomainException("A descrição deve ter no máximo 200 caracteres.");
        if (prazo <= DateTime.UtcNow)
            throw new DomainException("O prazo deve ser uma data/hora futura.");
    }
}