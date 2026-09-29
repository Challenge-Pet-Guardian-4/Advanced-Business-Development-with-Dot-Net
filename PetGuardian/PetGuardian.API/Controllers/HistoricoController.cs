using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Linha do tempo de eventos de um Pet (ex.: tarefas concluídas, marcos de saúde).</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class HistoricoController(IHistoricoService historicoService, ILogger<HistoricoController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de histórico cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/historico: Listando todos os históricos.");
        return Ok(historicoService.GetAll());
    }

    /// <summary>Obtém um registro de histórico pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/historico/{Id}: Buscando histórico.", id);
        var h = historicoService.GetById(id);
        if (h is null)
        {
            logger.LogWarning("HTTP GET /api/historico/{Id}: Histórico não encontrado.", id);
            return NotFound();
        }
        return Ok(h);
    }

    /// <summary>Lista o histórico de um pet específico.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/historico/by-pet/{PetId}: Buscando histórico do pet.", petId);
        return Ok(historicoService.GetByPetId(petId));
    }

    /// <summary>Cadastra um novo registro de histórico na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] HistoricoRequest request)
    {
        logger.LogInformation("HTTP POST /api/historico: Cadastrando evento de histórico '{TipoEvento}' para Pet {PetId}.", request.TipoHist, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/historico: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = historicoService.Create(request);
        logger.LogInformation("HTTP POST /api/historico: Histórico {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um registro de histórico existente (o pet vinculado não é reatribuível por aqui).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] HistoricoUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/historico/{Id}: Atualizando histórico.", id);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/historico/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = historicoService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/historico/{Id}: Histórico não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/historico/{Id}: Histórico atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove um registro de histórico pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/historico/{Id}: Excluindo histórico.", id);
        if (!historicoService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/historico/{Id}: Histórico não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/historico/{Id}: Histórico excluído com sucesso.", id);
        return NoContent();
    }
}