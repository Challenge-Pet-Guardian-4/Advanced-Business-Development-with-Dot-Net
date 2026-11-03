using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Vínculo N:N entre usuário e pet, com flag de responsável principal.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioPetController(IUsuarioPetService usuarioPetService) : ApiControllerBase
{
    /// <summary>Lista todos os vínculos da rede de cuidado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(usuarioPetService.GetAll().Select(ComLinks).ToList());

    /// <summary>Vínculos de um usuário.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId) =>
        Ok(usuarioPetService.GetByUsuarioId(usuarioId).Select(ComLinks).ToList());

    /// <summary>Vínculos de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioPetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId) =>
        Ok(usuarioPetService.GetByPetId(petId).Select(ComLinks).ToList());

    /// <summary>Rede de cuidado colaborativo (co-cuidadores e pets) de um usuário.</summary>
    [HttpGet("rede-cuidado/{usuarioId:guid}")]
    [ProducesResponseType(typeof(RedeCuidadoResponse), StatusCodes.Status200OK)]
    public IActionResult GetRedeCuidado(Guid usuarioId) =>
        Ok(usuarioPetService.GetRedeCuidadoByUsuarioId(usuarioId));

    /// <summary>Cria um vínculo usuário-pet (o próprio usuário do token ou Admin).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] UsuarioPetRequest request)
    {
        if (!IsSelfOrAdmin(request.UsuarioId))
            return Forbid();

        var created = usuarioPetService.Create(request);
        return CreatedAtAction(nameof(GetByUsuario), new { usuarioId = created.UsuarioId }, ComLinks(created));
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

        var created = usuarioPetService.InviteByUsuario(request);
        return CreatedAtAction(nameof(GetByPet), new { petId = created.PetId }, ComLinks(created));
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

        var created = usuarioPetService.InviteByEmail(request);
        return CreatedAtAction(nameof(GetByPet), new { petId = created.PetId }, ComLinks(created));
    }

    /// <summary>Alterna se este usuário é o responsável principal do pet.</summary>
    [HttpPut("{usuarioId:guid}/{petId:guid}")]
    [ProducesResponseType(typeof(UsuarioPetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid usuarioId, Guid petId, [FromBody] UsuarioPetUpdateRequest request)
    {
        if (!CanAccessPetCareCircle(usuarioPetService, petId))
            return Forbid();

        return usuarioPetService.Update(usuarioId, petId, request) is { } updated
            ? Ok(ComLinks(updated))
            : NotFound();
    }

    /// <summary>Remove um vínculo pela chave composta (usuarioId + petId).</summary>
    [HttpDelete("{usuarioId:guid}/{petId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid usuarioId, Guid petId)
    {
        if (!CanAccessPetCareCircle(usuarioPetService, petId))
            return Forbid();

        return usuarioPetService.Delete(usuarioId, petId) ? NoContent() : NotFound();
    }

    private UsuarioPetResponse ComLinks(UsuarioPetResponse up) => up with
    {
        Links =
        [
            LinkTo("usuario-vinculos", "GET", nameof(GetByUsuario), new { usuarioId = up.UsuarioId }),
            LinkTo("pet-vinculos", "GET", nameof(GetByPet), new { petId = up.PetId }),
            LinkTo("rede-cuidado", "GET", nameof(GetRedeCuidado), new { usuarioId = up.UsuarioId }),
            LinkTo("update", "PUT", nameof(Update), new { usuarioId = up.UsuarioId, petId = up.PetId }),
            LinkTo("delete", "DELETE", nameof(Delete), new { usuarioId = up.UsuarioId, petId = up.PetId }),
            LinkTo("usuario", "GET", "GetById", new { id = up.UsuarioId }, "Usuario"),
            LinkTo("pet", "GET", "GetById", new { id = up.PetId }, "Pet")
        ]
    };
}
