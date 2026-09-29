namespace PetGuardian.Application.DTOs;

/// <summary>
/// Resposta de autenticação contendo token JWT emitido e dados do perfil do usuário autenticado.
/// Compatível com o contrato do aplicativo Mobile (campo "User").
/// </summary>
public record LoginResponse(
    string Token,
    string Tipo,
    int ExpiraEmSegundos,
    UsuarioResponse User
);
