using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Módulos de uma Trilha. Agrupam Aulas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ModuloController(IModuloService moduloService, ILogger<ModuloController> logger) : ControllerBase
{
    /// <summary>Lista todos os módulos cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModuloResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/modulo: Listando todos os módulos.");
        return Ok(moduloService.GetAll());
    }

    /// <summary>Obtém um módulo pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/modulo/{Id}: Buscando módulo.", id);
        var modulo = moduloService.GetById(id);
        if (modulo is null)
        {
            logger.LogWarning("HTTP GET /api/modulo/{Id}: Módulo não encontrado.", id);
            return NotFound();
        }
        return Ok(modulo);
    }

    /// <summary>Lista módulos de uma trilha.</summary>
    [HttpGet("by-trilha/{trilhaId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuloResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByTrilha(Guid trilhaId)
    {
        logger.LogInformation("HTTP GET /api/modulo/by-trilha/{TrilhaId}: Buscando módulos da trilha.", trilhaId);
        return Ok(moduloService.GetByTrilhaId(trilhaId));
    }

    /// <summary>Cadastra um novo módulo na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] ModuloRequest request)
    {
        logger.LogInformation("HTTP POST /api/modulo: Cadastrando módulo '{Nome}' para Trilha {TrilhaId}.", request.Nome, request.TrilhaId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/modulo: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = moduloService.Create(request);
        logger.LogInformation("HTTP POST /api/modulo: Módulo {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um módulo existente (a trilha vinculada não é reatribuível por aqui).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] ModuloUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/modulo/{Id}: Atualizando módulo '{Nome}'.", id, request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/modulo/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = moduloService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/modulo/{Id}: Módulo não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/modulo/{Id}: Módulo atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove um módulo pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/modulo/{Id}: Excluindo módulo.", id);
        if (!moduloService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/modulo/{Id}: Módulo não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/modulo/{Id}: Módulo excluído com sucesso.", id);
        return NoContent();
    }
}