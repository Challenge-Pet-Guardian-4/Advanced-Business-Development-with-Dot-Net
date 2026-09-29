namespace PetGuardian.Application.DTOs;

public record UsuarioScoreResponse(
    Guid UsuarioId,
    int PontosTotais
);
