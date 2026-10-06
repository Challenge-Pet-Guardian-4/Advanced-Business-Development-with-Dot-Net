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
    IUsuarioQueryService usuarioQueryService,
    ILogger<UsuarioController> logger) : ApiControllerBase
{
    /// <summary>Lista usuários com paginação, ordenação e filtros (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAll([FromQuery] PageQuery page, [FromQuery] UsuarioFilter filter)
    {
        logger.LogInformation("HTTP GET /api/usuario: pesquisando usuários (página {Page}, tamanho {PageSize}).", page.Page, page.PageSize);
        var resultado = usuarioQueryService.Search(page, filter);

        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = page.SortBy,
            sortDir = page.SortDir,
            nome = filter.Nome,
            email = filter.Email,
            role = filter.Role
        }, ComLinks));
    }

    /// <summary>Obtém um usuário pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/usuario/{Id}: Buscando usuário.", id);
        var u = usuarioService.GetById(id);
        if (u is null)
        {
            logger.LogWarning("HTTP GET /api/usuario/{Id}: Usuário não encontrado.", id);
            return NotFound();
        }
        return Ok(ComLinks(u));
    }

    /// <summary>Obtém um usuário pelo e-mail.</summary>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByEmail([FromQuery] string email)
    {
        logger.LogInformation("HTTP GET /api/usuario/by-email: Buscando usuário por e-mail {Email}.", email);
        var u = usuarioService.GetByEmail(email);
        if (u is null)
        {
            logger.LogWarning("HTTP GET /api/usuario/by-email: Usuário com e-mail {Email} não encontrado.", email);
            return NotFound();
        }
        return Ok(ComLinks(u));
    }

    /// <summary>Score cumulativo do usuário (usuário inexistente retorna 400 via GlobalExceptionHandler).</summary>
    [HttpGet("{id:guid}/score")]
    [ProducesResponseType(typeof(UsuarioScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetScore(Guid id)
    {
        logger.LogInformation("HTTP GET /api/usuario/{Id}/score: Buscando score do usuário.", id);
        return Ok(usuarioService.GetScore(id));
    }

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
        logger.LogInformation("HTTP POST /api/usuario: Iniciando cadastro do usuário '{Nome}' ({Email}).", request.Nome, request.Email);

        if (request.Role == RoleUsuario.Admin && !IsAdmin)
        {
            logger.LogWarning("HTTP POST /api/usuario: tentativa de criar Admin sem privilégio ({Email}).", request.Email);
            return Forbid();
        }

        var created = usuarioService.Create(request);
        logger.LogInformation("HTTP POST /api/usuario: Usuário {Id} cadastrado com sucesso.", created.Id);
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
        {
            logger.LogWarning("HTTP PUT /api/usuario/{Id}: operação negada ao usuário {CurrentUserId}.", id, CurrentUserId);
            return Forbid();
        }

        logger.LogInformation("HTTP PUT /api/usuario/{Id}: Atualizando usuário '{Nome}'.", id, request.Nome);
        var updated = usuarioService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/usuario/{Id}: Usuário não encontrado para atualização.", id);
            return NotFound();
        }
        return Ok(ComLinks(updated));
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

        logger.LogInformation("HTTP DELETE /api/usuario/{Id}: Excluindo usuário.", id);
        if (!usuarioService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/usuario/{Id}: Usuário não encontrado para exclusão.", id);
            return NotFound();
        }
        return NoContent();
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