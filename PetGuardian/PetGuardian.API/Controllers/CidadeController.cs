using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Cidades. Pertencem a um estado.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CidadeController(ICidadeService cidadeService, ILogger<CidadeController> logger) : ControllerBase
{
    /// <summary>Lista todas as cidades.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CidadeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/cidade: Listando todas as cidades.");
        return Ok(cidadeService.GetAll());
    }

    /// <summary>Obtém uma cidade pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/cidade/{Id}: Buscando cidade.", id);
        var cidade = cidadeService.GetById(id);
        if (cidade is null)
        {
            logger.LogWarning("HTTP GET /api/cidade/{Id}: Cidade não encontrada.", id);
            return NotFound();
        }
        return Ok(cidade);
    }

    /// <summary>Lista cidades de um estado.</summary>
    [HttpGet("by-estado/{estadoId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CidadeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByEstado(Guid estadoId)
    {
        logger.LogInformation("HTTP GET /api/cidade/by-estado/{EstadoId}: Buscando cidades por estado.", estadoId);
        return Ok(cidadeService.GetByEstadoId(estadoId));
    }

    /// <summary>Cria uma cidade.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CidadeRequest request)
    {
        logger.LogInformation("HTTP POST /api/cidade: Cadastrando cidade '{Nome}' no Estado {EstadoId}.", request.NomeCidade, request.EstadoId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/cidade: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = cidadeService.Create(request);
        logger.LogInformation("HTTP POST /api/cidade: Cidade {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza uma cidade existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] CidadeRequest request)
    {
        logger.LogInformation("HTTP PUT /api/cidade/{Id}: Atualizando cidade '{Nome}'.", id, request.NomeCidade);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/cidade/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = cidadeService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/cidade/{Id}: Cidade não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/cidade/{Id}: Cidade atualizada com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove uma cidade pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/cidade/{Id}: Excluindo cidade.", id);
        if (!cidadeService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/cidade/{Id}: Cidade não encontrada para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/cidade/{Id}: Cidade excluída com sucesso.", id);
        return NoContent();
    }
}