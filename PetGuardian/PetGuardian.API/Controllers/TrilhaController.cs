using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Trilhas de cuidado/aprendizado vinculadas a um Pet. Agrupam Módulos.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TrilhaController(ITrilhaService trilhaService, ILogger<TrilhaController> logger) : ControllerBase
{
    /// <summary>Lista todas as trilhas cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TrilhaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/trilha: Listando todas as trilhas.");
        return Ok(trilhaService.GetAll());
    }

    /// <summary>Obtém uma trilha pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/trilha/{Id}: Buscando trilha.", id);
        var trilha = trilhaService.GetById(id);
        if (trilha is null)
        {
            logger.LogWarning("HTTP GET /api/trilha/{Id}: Trilha não encontrada.", id);
            return NotFound();
        }
        return Ok(trilha);
    }

    /// <summary>Lista trilhas de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrilhaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/trilha/by-pet/{PetId}: Buscando trilhas do pet.", petId);
        return Ok(trilhaService.GetByPetId(petId));
    }

    /// <summary>Cadastra uma nova trilha na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TrilhaRequest request)
    {
        logger.LogInformation("HTTP POST /api/trilha: Cadastrando trilha '{Nome}' para Pet {PetId}.", request.Nome, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/trilha: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = trilhaService.Create(request);
        logger.LogInformation("HTTP POST /api/trilha: Trilha {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza uma trilha existente (o pet vinculado não é reatribuível por aqui).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TrilhaUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/trilha/{Id}: Atualizando trilha '{Nome}'.", id, request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/trilha/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = trilhaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/trilha/{Id}: Trilha não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/trilha/{Id}: Trilha atualizada com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove uma trilha pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/trilha/{Id}: Excluindo trilha.", id);
        if (!trilhaService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/trilha/{Id}: Trilha não encontrada para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/trilha/{Id}: Trilha excluída com sucesso.", id);
        return NoContent();
    }
}