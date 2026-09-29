using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Aulas de um Módulo. Concedem pontos de gamificação ao ser concluídas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AulaController(IAulaService aulaService, ILogger<AulaController> logger) : ControllerBase
{
    /// <summary>Lista todas as aulas cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AulaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/aula: Listando todas as aulas.");
        return Ok(aulaService.GetAll());
    }

    /// <summary>Obtém uma aula pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/aula/{Id}: Buscando aula.", id);
        var aula = aulaService.GetById(id);
        if (aula is null)
        {
            logger.LogWarning("HTTP GET /api/aula/{Id}: Aula não encontrada.", id);
            return NotFound();
        }
        return Ok(aula);
    }

    /// <summary>Lista aulas de um módulo.</summary>
    [HttpGet("by-modulo/{moduloId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AulaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByModulo(Guid moduloId)
    {
        logger.LogInformation("HTTP GET /api/aula/by-modulo/{ModuloId}: Buscando aulas do módulo.", moduloId);
        return Ok(aulaService.GetByModuloId(moduloId));
    }

    /// <summary>Cadastra uma nova aula na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] AulaRequest request)
    {
        logger.LogInformation("HTTP POST /api/aula: Cadastrando aula '{Nome}' para Módulo {ModuloId}.", request.Nome, request.ModuloId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/aula: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = aulaService.Create(request);
        logger.LogInformation("HTTP POST /api/aula: Aula {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza uma aula existente (o módulo vinculado não é reatribuível por aqui).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] AulaUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/aula/{Id}: Atualizando aula '{Nome}'.", id, request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/aula/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = aulaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/aula/{Id}: Aula não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/aula/{Id}: Aula atualizada com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove uma aula pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/aula/{Id}: Excluindo aula.", id);
        if (!aulaService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/aula/{Id}: Aula não encontrada para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/aula/{Id}: Aula excluída com sucesso.", id);
        return NoContent();
    }
}