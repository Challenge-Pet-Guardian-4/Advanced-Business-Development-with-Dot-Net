using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Enums;

namespace PetGuardian.API.Controllers;

/// <summary>Usuários. Senha nunca é exposta nas respostas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioController(
    IUsuarioService usuarioService,
    IUsuarioPetService usuarioPetService) : ApiControllerBase
{
    /// <summary>Obtém o perfil do usuário autenticado através do token JWT.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetMe() =>
        CurrentUserId is not { } userId
            ? Unauthorized()
            : usuarioService.GetById(userId) is { } u ? Ok(ComLinks(u)) : NotFound();

    /// <summary>Atualiza o perfil do próprio usuário autenticado.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateMe([FromBody] UsuarioUpdateRequest request) =>
        CurrentUserId is not { } userId
            ? Unauthorized()
            : usuarioService.Update(userId, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Rede de cuidado colaborativo do usuário autenticado.</summary>
    [HttpGet("me/rede-cuidado")]
    [ProducesResponseType(typeof(RedeCuidadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyRedeCuidado() =>
        CurrentUserId is { } userId
            ? Ok(usuarioPetService.GetRedeCuidadoByUsuarioId(userId))
            : Unauthorized();

    /// <summary>Atualiza o usuário autenticado para o perfil Premium.</summary>
    [HttpPatch("me/upgrade-premium")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpgradePremium() =>
        CurrentUserId is { } userId
            ? Ok(ComLinks(usuarioService.UpgradePremium(userId)))
            : Unauthorized();

    /// <summary>Lista usuários com paginação, ordenação e filtros (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] UsuarioFilter filter)
    {
        var resultado = usuarioService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            nome = filter.Nome,
            email = filter.Email,
            role = filter.Role
        }, ComLinks));
    }

    /// <summary>Obtém um usuário pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        usuarioService.GetById(id) is { } u ? Ok(ComLinks(u)) : NotFound();

    /// <summary>Obtém um usuário pelo e-mail.</summary>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByEmail([FromQuery] string email) =>
        usuarioService.GetByEmail(email) is { } u ? Ok(ComLinks(u)) : NotFound();

    /// <summary>Score cumulativo do usuário (usuário inexistente retorna 400 via GlobalExceptionHandler).</summary>
    [HttpGet("{id:guid}/score")]
    [ProducesResponseType(typeof(UsuarioScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetScore(Guid id) => Ok(usuarioService.GetScore(id));

    /// <summary>
    /// Cadastro (anônimo, é a tela de sign-up). Criar perfil Admin exige um token Admin.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] UsuarioRequest request)
    {
        if (!IsAdmin && request.Role != RoleUsuario.Comum)
            return Forbid();

        var created = usuarioService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza nome/e-mail/senha/perfil. Somente o próprio usuário ou Admin; só Admin define o perfil Admin.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UsuarioUpdateRequest request)
    {
        if (!IsSelfOrAdmin(id) || (request.Role == RoleUsuario.Admin && !IsAdmin))
            return Forbid();

        return usuarioService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();
    }

    /// <summary>Exclui um usuário (o próprio usuário ou Admin).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (!IsSelfOrAdmin(id))
            return Forbid();

        return usuarioService.Delete(id) ? NoContent() : NotFound();
    }

    private UsuarioResponse ComLinks(UsuarioResponse u) => u with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = u.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = u.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = u.Id }),
            LinkTo("score", "GET", nameof(GetScore), new { id = u.Id }),
            LinkTo("tarefas", "GET", "GetByUsuario", new { usuarioId = u.Id }, "Tarefa"),
            LinkTo("rede-cuidado", "GET", "GetRedeCuidado", new { usuarioId = u.Id }, "UsuarioPet")
        ]
    };
}
