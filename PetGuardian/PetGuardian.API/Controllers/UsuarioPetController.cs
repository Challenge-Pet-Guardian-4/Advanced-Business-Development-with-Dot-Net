using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Vínculo N:N entre usuário e pet, com flag de responsável principal.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioPetController(IUsuarioPetService usuarioPetService, ILogger<UsuarioPetController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de vínculos de rede de cuidado de usuários e pets cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/usuariopet: Listando todos os vínculos de rede de cuidado.");
        return Ok(usuarioPetService.GetAll());
    }

    /// <summary>Lista todos os registros de vínculos de rede de cuidado de usuários e pets associados a um usuário específico.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/by-usuario/{UsuarioId}: Buscando vínculos por usuário.", usuarioId);
        return Ok(usuarioPetService.GetByUsuarioId(usuarioId));
    }

    /// <summary>Lista todos os registros de vínculos de rede de cuidado de usuários e pets associados a um pet específico.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/by-pet/{PetId}: Buscando vínculos por pet.", petId);
        return Ok(usuarioPetService.GetByPetId(petId));
    }

    /// <summary>Obtém a rede de cuidado colaborativo (co-cuidadores e pets vinculados) de um usuário.</summary>
    [HttpGet("rede-cuidado/{usuarioId:guid}")]
    [ProducesResponseType(typeof(RedeCuidadoResponse), StatusCodes.Status200OK)]
    public IActionResult GetRedeCuidado(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/usuariopet/rede-cuidado/{UsuarioId}: Montando rede de cuidado.", usuarioId);
        return Ok(usuarioPetService.GetRedeCuidadoByUsuarioId(usuarioId));
    }

    /// <summary>Cadastra um novo registro de vínculo de rede de cuidado de usuário e pet na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] UsuarioPetRequest request)
    {
        logger.LogInformation("HTTP POST /api/usuariopet: Vinculando Usuário {UsuarioId} ao Pet {PetId}.", request.UsuarioId, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/usuariopet: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = usuarioPetService.Create(request);
        logger.LogInformation("HTTP POST /api/usuariopet: Vínculo criado com sucesso.");
        return CreatedAtAction(nameof(GetByUsuario), new { usuarioId = created.UsuarioId }, created);
    }

    /// <summary>Envia um convite de participação na rede de cuidado por ID (Exclusivo para Responsável Principal).</summary>
    [HttpPost("invite/by-usuario")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult InviteByUsuario([FromBody] UsuarioPetInviteByUsuarioRequest request)
    {
        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-usuario: Convite do Admin {AdminId} para Usuário {ConvidadoId} no Pet {PetId}.", request.AdminUsuarioId, request.UsuarioConvidadoId, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/usuariopet/invite/by-usuario: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = usuarioPetService.InviteByUsuario(request);
        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-usuario: Convite aceito e vínculo criado.");
        return CreatedAtAction(nameof(GetByPet), new { petId = created.PetId }, created);
    }

    /// <summary>Envia um convite de participação na rede de cuidado buscando por E-mail (Exclusivo para Responsável Principal).</summary>
    [HttpPost("invite/by-email")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult InviteByEmail([FromBody] UsuarioPetInviteByEmailRequest request)
    {
        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-email: Convite do Admin {AdminId} para E-mail {Email} no Pet {PetId}.", request.AdminUsuarioId, request.Email, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/usuariopet/invite/by-email: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = usuarioPetService.InviteByEmail(request);
        logger.LogInformation("HTTP POST /api/usuariopet/invite/by-email: Convite aceito e vínculo criado.");
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
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/usuariopet/{UsuarioId}/{PetId}: ModelState inválido.", usuarioId, petId);
            return BadRequest(ModelState);
        }
        var updated = usuarioPetService.Update(usuarioId, petId, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/usuariopet/{UsuarioId}/{PetId}: Vínculo não encontrado para atualização.", usuarioId, petId);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/usuariopet/{UsuarioId}/{PetId}: Vínculo atualizado com sucesso.", usuarioId, petId);
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
        logger.LogInformation("HTTP DELETE /api/usuariopet/{UsuarioId}/{PetId}: Vínculo excluído com sucesso.", usuarioId, petId);
        return NoContent();
    }
}
