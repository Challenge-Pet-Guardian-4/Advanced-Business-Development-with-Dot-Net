using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Autenticação e emissão de tokens JWT.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ApiControllerBase
{
    /// <summary>
    /// Autentica com e-mail e senha. Suporta /api/auth/login (RESTful) e /login (app Mobile).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [HttpPost("/login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var resposta = authService.Login(request);
        if (resposta is null)
            return Unauthorized(new { message = "E-mail ou senha incorretos." });

        logger.LogInformation("HTTP POST /login: usuário {Id} autenticado.", resposta.User.Id);
        return Ok(resposta);
    }

    /// <summary>Perfil do usuário autenticado (claims do Bearer token).</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Me()
    {
        if (CurrentUserId is not { } usuarioId)
        {
            logger.LogWarning("HTTP GET /api/auth/me: claim de identificação ausente ou inválida.");
            return Unauthorized();
        }

        var perfil = authService.GetPerfil(usuarioId);
        return perfil is null ? NotFound() : Ok(perfil);
    }
}