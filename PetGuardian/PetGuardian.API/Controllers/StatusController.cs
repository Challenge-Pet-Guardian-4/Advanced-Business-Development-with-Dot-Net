using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Status de ciclo de vida das tarefas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class StatusController(IStatusService statusService, ILogger<StatusController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de status cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StatusResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/status: Listando todos os status.");
        return Ok(statusService.GetAll());
    }

    /// <summary>Obtém um registro de status pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/status/{Id}: Buscando status.", id);
        var status = statusService.GetById(id);
        if (status is null)
        {
            logger.LogWarning("HTTP GET /api/status/{Id}: Status não encontrado.", id);
            return NotFound();
        }
        return Ok(status);
    }

    /// <summary>Cadastra um novo registro de status na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] StatusRequest request)
    {
        logger.LogInformation("HTTP POST /api/status: Cadastrando status '{Nome}'.", request.NomeStatus);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/status: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = statusService.Create(request);
        logger.LogInformation("HTTP POST /api/status: Status {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um status existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] StatusRequest request)
    {
        logger.LogInformation("HTTP PUT /api/status/{Id}: Atualizando status '{Nome}'.", id, request.NomeStatus);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/status/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = statusService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/status/{Id}: Status não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/status/{Id}: Status atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Exclui um registro de status cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/status/{Id}: Excluindo status.", id);
        if (!statusService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/status/{Id}: Status não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/status/{Id}: Status excluído com sucesso.", id);
        return NoContent();
    }
}