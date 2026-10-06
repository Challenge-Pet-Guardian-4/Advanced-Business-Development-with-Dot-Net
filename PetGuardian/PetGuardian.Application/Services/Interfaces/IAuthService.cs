using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Retorna null quando e-mail ou senha estão incorretos.</summary>
    LoginResponse? Login(LoginRequest request);

    UsuarioResponse? GetPerfil(Guid usuarioId);
}