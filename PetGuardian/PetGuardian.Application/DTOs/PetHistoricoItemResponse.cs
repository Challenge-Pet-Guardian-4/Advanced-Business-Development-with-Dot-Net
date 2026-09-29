namespace PetGuardian.Application.DTOs;

/// <summary>Reescrito para não depender mais de Atendimento (removido).</summary>
public record PetHistoricoItemResponse(
    DateTime DataEvento,
    string   TipoEvento,
    Guid     ReferenciaId,
    string   Titulo,
    string?  Descricao,
    Guid     PetId,
    Guid?    UsuarioExecutorId,
    int?     PontosTarefa
);