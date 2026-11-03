using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Autenticação e emissão de tokens JWT.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>
    /// Autentica com e-mail e senha e retorna o token JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request) =>
        authService.Login(request) is { } resposta
            ? Ok(resposta)
            : Unauthorized(new { message = "E-mail ou senha incorretos." });

    /// <summary>Perfil do usuário autenticado (claims do Bearer token).</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Me()
    {
        if (CurrentUserId is not { } usuarioId)
            return Unauthorized();

        if (authService.GetPerfil(usuarioId) is not { } perfil)
            return NotFound();

        return Ok(perfil with
        {
            Links =
            [
                LinkTo("self", "GET", nameof(Me)),
                LinkTo("usuario", "GET", "GetById", new { id = perfil.Id }, "Usuario"),
                LinkTo("score", "GET", "GetScore", new { id = perfil.Id }, "Usuario")
            ]
        });
    }
}
