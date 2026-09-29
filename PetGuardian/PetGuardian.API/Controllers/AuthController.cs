using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>
/// Autenticação e emissão de tokens JWT para tutores e administradores da plataforma.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController(
    IUsuarioRepository usuarioRepository,
    ITokenService tokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Autentica o usuário com e-mail e senha, retornando token JWT e perfil do usuário.
    /// Suporta tanto /api/auth/login (padrão RESTful) quanto /login (integração direta com o app Mobile).
    /// </summary>
    [HttpPost("login")]
    [HttpPost("/login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        logger.LogInformation("HTTP POST /login: Tentativa de login para {Email}.", request.Email);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = usuarioRepository.GetByEmail(request.Email.Trim().ToLower());
        if (usuario is null)
        {
            logger.LogWarning("HTTP POST /login: E-mail {Email} não cadastrado.", request.Email);
            return Unauthorized(new { message = "E-mail ou senha incorretos." });
        }

        var senhaValida = usuario.VerifyPassword(request.Senha);
        if (!senhaValida)
        {
            logger.LogWarning("HTTP POST /login: Senha inválida para o usuário {Email}.", request.Email);
            return Unauthorized(new { message = "E-mail ou senha incorretos." });
        }

        var token = tokenService.GerarToken(usuario);
        var userResponse = new UsuarioResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.TelefoneId
        );

        logger.LogInformation("HTTP POST /login: Usuário {Id} autenticado com sucesso.", usuario.Id);
        return Ok(new LoginResponse(token, "Bearer", 7200, userResponse));
    }

    /// <summary>
    /// Retorna os dados do usuário atualmente autenticado a partir das claims do Token Bearer.
    /// Exige cabeçalho Authorization: Bearer {token}.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim) || !Guid.TryParse(idClaim, out var usuarioId))
        {
            logger.LogWarning("HTTP GET /api/auth/me: Claim de identificação do usuário ausente ou inválida.");
            return Unauthorized();
        }

        var usuario = usuarioRepository.GetById(usuarioId);
        if (usuario is null)
        {
            logger.LogWarning("HTTP GET /api/auth/me: Usuário {Id} não encontrado no banco.", usuarioId);
            return NotFound();
        }

        var userResponse = new UsuarioResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.TelefoneId
        );

        return Ok(userResponse);
    }
}
