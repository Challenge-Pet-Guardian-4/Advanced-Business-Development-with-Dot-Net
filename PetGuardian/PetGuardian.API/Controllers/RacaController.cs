using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Raças de pets.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class RacaController(IRacaService racaService, ILogger<RacaController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de raças cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RacaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/raca: Listando todas as raças.");
        return Ok(racaService.GetAll());
    }

    /// <summary>Obtém um registro de raça pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/raca/{Id}: Buscando raça.", id);
        var raca = racaService.GetById(id);
        if (raca is null)
        {
            logger.LogWarning("HTTP GET /api/raca/{Id}: Raça não encontrada.", id);
            return NotFound();
        }
        return Ok(raca);
    }

    /// <summary>Cadastra um novo registro de raça na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] RacaRequest request)
    {
        logger.LogInformation("HTTP POST /api/raca: Cadastrando raça '{Nome}'.", request.NomeRaca);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/raca: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = racaService.Create(request);
        logger.LogInformation("HTTP POST /api/raca: Raça {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza uma raça existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] RacaRequest request)
    {
        logger.LogInformation("HTTP PUT /api/raca/{Id}: Atualizando raça '{Nome}'.", id, request.NomeRaca);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/raca/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = racaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/raca/{Id}: Raça não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/raca/{Id}: Raça atualizada com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Exclui um registro de raça cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/raca/{Id}: Excluindo raça.", id);
        if (!racaService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/raca/{Id}: Raça não encontrada para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/raca/{Id}: Raça excluída com sucesso.", id);
        return NoContent();
    }
}