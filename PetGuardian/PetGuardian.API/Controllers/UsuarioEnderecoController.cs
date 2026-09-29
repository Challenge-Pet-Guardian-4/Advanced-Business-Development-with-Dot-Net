using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Vínculo N:N entre usuário e endereço.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioEnderecoController(IUsuarioEnderecoService usuarioEnderecoService, ILogger<UsuarioEnderecoController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de vínculos de endereços de usuários cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioEnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/usuarioendereco: Listando todos os vínculos usuário-endereço.");
        return Ok(usuarioEnderecoService.GetAll());
    }

    /// <summary>Lista todos os registros de vínculos de endereços de usuários associados a um usuário específico.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioEnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/usuarioendereco/by-usuario/{UsuarioId}: Buscando vínculos por usuário.", usuarioId);
        return Ok(usuarioEnderecoService.GetByUsuarioId(usuarioId));
    }

    /// <summary>Lista todos os registros de vínculos de endereços de usuários associados a um endereço específico.</summary>
    [HttpGet("by-endereco/{enderecoId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioEnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByEndereco(Guid enderecoId)
    {
        logger.LogInformation("HTTP GET /api/usuarioendereco/by-endereco/{EnderecoId}: Buscando vínculos por endereço.", enderecoId);
        return Ok(usuarioEnderecoService.GetByEnderecoId(enderecoId));
    }

    /// <summary>Cadastra um novo registro de vínculo de endereço de usuário na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioEnderecoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] UsuarioEnderecoRequest request)
    {
        logger.LogInformation("HTTP POST /api/usuarioendereco: Vinculando Usuário {UsuarioId} ao Endereço {EnderecoId}.", request.UsuarioId, request.EnderecoId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/usuarioendereco: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = usuarioEnderecoService.Create(request);
        logger.LogInformation("HTTP POST /api/usuarioendereco: Vínculo usuário-endereço criado com sucesso.");
        return CreatedAtAction(nameof(GetByUsuario), new { usuarioId = created.UsuarioId }, created);
    }

    /// <summary>Remove um vínculo pela chave composta.</summary>
    [HttpDelete("{usuarioId:guid}/{enderecoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid usuarioId, Guid enderecoId)
    {
        logger.LogInformation("HTTP DELETE /api/usuarioendereco/{UsuarioId}/{EnderecoId}: Excluindo vínculo usuário-endereço.", usuarioId, enderecoId);
        if (!usuarioEnderecoService.Delete(usuarioId, enderecoId))
        {
            logger.LogWarning("HTTP DELETE /api/usuarioendereco/{UsuarioId}/{EnderecoId}: Vínculo não encontrado para exclusão.", usuarioId, enderecoId);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/usuarioendereco/{UsuarioId}/{EnderecoId}: Vínculo excluído com sucesso.", usuarioId, enderecoId);
        return NoContent();
    }
}