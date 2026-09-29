using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Gerenciamento de pets no contexto de rede de cuidado.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PetController(IPetService petService, ILogger<PetController> logger) : ControllerBase
{
    /// <summary>Lista todos os pets.</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/pet: Listando todos os pets.");
        return Ok(petService.GetAll());
    }

    /// <summary>Obtém um pet pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/pet/{Id}: Buscando pet.", id);
        var pet = petService.GetById(id);
        if (pet is null)
        {
            logger.LogWarning("HTTP GET /api/pet/{Id}: Pet não encontrado.", id);
            return NotFound();
        }
        return Ok(pet);
    }

    /// <summary>Lista todos os pets de uma raça.</summary>
    [HttpGet("by-raca/{racaId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByRaca(Guid racaId)
    {
        logger.LogInformation("HTTP GET /api/pet/by-raca/{RacaId}: Buscando pets por raça.", racaId);
        return Ok(petService.GetByRacaId(racaId));
    }

    /// <summary>
    /// Retorna a linha do tempo histórica unificada de um pet.
    /// Passou de Atendimentos+Tarefas para Historico+Tarefas concluídas.
    /// </summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IReadOnlyList<PetHistoricoItemResponse>), StatusCodes.Status200OK)]
    public IActionResult GetHistorico(Guid id)
    {
        logger.LogInformation("HTTP GET /api/pet/{Id}/historico: Buscando histórico do pet.", id);
        return Ok(petService.GetHistorico(id));
    }

    /// <summary>Cadastra um pet.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] PetRequest request)
    {
        logger.LogInformation("HTTP POST /api/pet: Iniciando cadastro do pet '{Nome}'.", request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/pet: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = petService.Create(request);
        logger.LogInformation("HTTP POST /api/pet: Pet {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um pet existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] PetRequest request)
    {
        logger.LogInformation("HTTP PUT /api/pet/{Id}: Atualizando pet '{Nome}'.", id, request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/pet/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = petService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/pet/{Id}: Pet não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/pet/{Id}: Pet atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove um pet pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/pet/{Id}: Excluindo pet.", id);
        if (!petService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/pet/{Id}: Pet não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/pet/{Id}: Pet excluído com sucesso.", id);
        return NoContent();
    }
}
