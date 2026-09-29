namespace PetGuardian.Application.DTOs;

/// <summary>"Atendimentos" foi substituído por "Historico" em cada pet da rede (Atendimento removido do banco de dados).</summary>
public record RedeCuidadoResponse(
    Guid UsuarioId,
    IReadOnlyList<RedeCuidadoPetResponse> Pets,
    IReadOnlyList<RedeCuidadoCoCuidadorResponse> CoCuidadores
);

public record RedeCuidadoPetResponse(
    Guid PetId,
    string Nome,
    IReadOnlyList<RedeCuidadoTarefaResponse> Tarefas,
    IReadOnlyList<RedeCuidadoHistoricoResponse> Historico
);

public record RedeCuidadoTarefaResponse(
    Guid TarefaId,
    string Titulo,
    DateTime Prazo,
    DateTime? Conclusao,
    Guid UsuarioExecutorId,
    Guid StatusId,
    int PontosTarefa
);

public record RedeCuidadoHistoricoResponse(
    Guid HistoricoId,
    string TipoHist,
    DateTime DataHist
);

public record RedeCuidadoCoCuidadorResponse(
    Guid UsuarioId,
    string Nome,
    string Email
);