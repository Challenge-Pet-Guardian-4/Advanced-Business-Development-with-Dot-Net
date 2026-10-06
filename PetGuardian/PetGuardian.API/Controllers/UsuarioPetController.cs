using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Vínculo N:N entre usuário e pet, com flag de responsável principal.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioPetController(IUsuarioPetService usuarioPetService, ILogger<UsuarioPetController> logger) : ApiControllerBase
{
    /// <summary>Lista todos os vínculos da rede de cuidado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/usuariopet: Listando todos os vínculos de rede de cuidado.");
        return Ok(usuarioPetService.GetAll());
    }

    /// <summary>Vínculos de um usuário.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/by-usuario/{UsuarioId}: Buscando vínculos por usuário.", usuarioId);
        return Ok(usuarioPetService.GetByUsuarioId(usuarioId));
    }

    /// <summary>Vínculos de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/by-pet/{PetId}: Buscando vínculos por pet.", petId);
        return Ok(usuarioPetService.GetByPetId(petId));
    }

    /// <summary>Rede de cuidado colaborativo (co-cuidadores e pets) de um usuário.</summary>
    [HttpGet("rede-cuidado/{usuarioId:guid}")]
    [ProducesResponseType(typeof(RedeCuidadoResponse), StatusCodes.Status200OK)]
    public IActionResult GetRedeCuidado(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/rede-cuidado/{UsuarioId}: Montando rede de cuidado.", usuarioId);
        return Ok(usuarioPetService.GetRedeCuidadoByUsuarioId(usuarioId));
    }

    /// <summary>Cria um vínculo usuário-pet (o próprio usuário do token ou Admin).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] UsuarioPetRequest request)
    {
        if (!IsSelfOrAdmin(request.UsuarioId))
            return Forbid();

        logger.LogInformation("HTTP POST /api/usuariopet: Vinculando Usuário {UsuarioId} ao Pet {PetId}.", request.UsuarioId, request.PetId);
        var created = usuarioPetService.Create(request);
        return CreatedAtAction(nameof(GetByUsuario), new { usuarioId = created.UsuarioId }, created);
    }

    /// <summary>Convite por ID (exclusivo do responsável principal; AdminUsuarioId deve ser o usuário do token).</summary>
    [HttpPost("invite/by-usuario")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult InviteByUsuario([FromBody] UsuarioPetInviteByUsuarioRequest request)
    {
        if (!IsSelfOrAdmin(request.AdminUsuarioId))
            return Forbid();

        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-usuario: Convite do Admin {AdminId} para Usuário {ConvidadoId} no Pet {PetId}.", request.AdminUsuarioId, request.UsuarioConvidadoId, request.PetId);
        var created = usuarioPetService.InviteByUsuario(request);
        return CreatedAtAction(nameof(GetByPet), new { petId = created.PetId }, created);
    }

    /// <summary>Convite por e-mail (exclusivo do responsável principal; AdminUsuarioId deve ser o usuário do token).</summary>
    [HttpPost("invite/by-email")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult InviteByEmail([FromBody] UsuarioPetInviteByEmailRequest request)
    {
        if (!IsSelfOrAdmin(request.AdminUsuarioId))
            return Forbid();

        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-email: Convite do Admin {AdminId} para E-mail {Email} no Pet {PetId}.", request.AdminUsuarioId, request.Email, request.PetId);
        var created = usuarioPetService.InviteByEmail(request);
        return CreatedAtAction(nameof(GetByPet), new { petId = created.PetId }, created);
    }

    /// <summary>Alterna se este usuário é o responsável principal do pet.</summary>
    [HttpPut("{usuarioId:guid}/{petId:guid}")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid usuarioId, Guid petId, [FromBody] UsuarioPetUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/usuariopet/{UsuarioId}/{PetId}: Atualizando responsabilidade principal.", usuarioId, petId);
        var updated = usuarioPetService.Update(usuarioId, petId, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/usuariopet/{UsuarioId}/{PetId}: Vínculo não encontrado para atualização.", usuarioId, petId);
            return NotFound();
        }
        return Ok(updated);
    }

    /// <summary>Remove um vínculo pela chave composta (usuarioId + petId).</summary>
    [HttpDelete("{usuarioId:guid}/{petId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid usuarioId, Guid petId)
    {
        logger.LogInformation("HTTP DELETE /api/usuariopet/{UsuarioId}/{PetId}: Desvinculando usuário de pet.", usuarioId, petId);
        if (!usuarioPetService.Delete(usuarioId, petId))
        {
            logger.LogWarning("HTTP DELETE /api/usuariopet/{UsuarioId}/{PetId}: Vínculo não encontrado para exclusão.", usuarioId, petId);
            return NotFound();
        }
        return NoContent();
    }
}