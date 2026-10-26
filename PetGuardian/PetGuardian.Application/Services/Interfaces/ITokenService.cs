using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Interfaces;

/// <summary>
/// Contrato do serviço emissor de tokens JWT (RFC 7519).
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um token JWT assinado digitalmente com HMAC-SHA256 contendo as claims do usuário.
    /// </summary>
    string GerarToken(Usuario usuario);
}
