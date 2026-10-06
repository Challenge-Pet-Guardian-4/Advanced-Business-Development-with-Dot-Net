using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>Autenticação (e-mail/senha) e emissão de JWT. Tira o acesso a repositório do AuthController.</summary>
public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public LoginResponse? Login(LoginRequest request)
    {
        logger.LogInformation("Tentativa de login para {Email}.", request.Email);

        var usuario = usuarioRepository.GetByEmail(request.Email.Trim().ToLowerInvariant());
        if (usuario is null)
        {
            logger.LogWarning("Login negado: e-mail {Email} não cadastrado.", request.Email);
            return null;
        }

        if (!usuario.VerifyPassword(request.Senha))
        {
            logger.LogWarning("Login negado: senha inválida para {Email}.", request.Email);
            return null;
        }

        var token = tokenService.GerarToken(usuario);
        logger.LogInformation("Usuário {UsuarioId} autenticado com sucesso.", usuario.Id);
        return new LoginResponse(token, "Bearer", TokenService.ExpirationHours * 3600, UsuarioResponse.FromDomain(usuario));
    }

    public UsuarioResponse? GetPerfil(Guid usuarioId)
    {
        var usuario = usuarioRepository.GetById(usuarioId);
        if (usuario is null)
            logger.LogWarning("Perfil solicitado para usuário inexistente: {UsuarioId}.", usuarioId);

        return usuario is null ? null : UsuarioResponse.FromDomain(usuario);
    }
}