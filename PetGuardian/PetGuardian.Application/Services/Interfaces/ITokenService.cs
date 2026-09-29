using System.Security.Claims;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Interfaces;

/// <summary>
/// Contrato do serviço emissor e validador de tokens JWT (RFC 7519).
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um token JWT assinado digitalmente com HMAC-SHA256 contendo as claims do usuário.
    /// </summary>
    string GerarToken(Usuario usuario);

    /// <summary>
    /// Valida a assinatura, formato e prazo de expiração do token JWT, extraindo as claims em caso de sucesso.
    /// </summary>
    (bool IsValido, ClaimsPrincipal? Principal) ValidarToken(string token);
}
