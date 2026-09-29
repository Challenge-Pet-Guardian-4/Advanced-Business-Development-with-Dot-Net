using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Bairros. Pertencem a uma cidade.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class BairroController(IBairroService bairroService, ILogger<BairroController> logger) : ControllerBase
{
    /// <summary>Lista todos os bairros.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BairroResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/bairro: Listando todos os bairros.");
        return Ok(bairroService.GetAll());
    }

    /// <summary>Obtém um bairro pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/bairro/{Id}: Buscando bairro.", id);
        var bairro = bairroService.GetById(id);
        if (bairro is null)
        {
            logger.LogWarning("HTTP GET /api/bairro/{Id}: Bairro não encontrado.", id);
            return NotFound();
        }
        return Ok(bairro);
    }

    /// <summary>Lista bairros de uma cidade.</summary>
    [HttpGet("by-cidade/{cidadeId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<BairroResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByCidade(Guid cidadeId)
    {
        logger.LogInformation("HTTP GET /api/bairro/by-cidade/{CidadeId}: Buscando bairros por cidade.", cidadeId);
        return Ok(bairroService.GetByCidadeId(cidadeId));
    }

    /// <summary>Cria um bairro.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] BairroRequest request)
    {
        logger.LogInformation("HTTP POST /api/bairro: Cadastrando bairro '{Nome}' na Cidade {CidadeId}.", request.NomeBairro, request.CidadeId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/bairro: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = bairroService.Create(request);
        logger.LogInformation("HTTP POST /api/bairro: Bairro {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um bairro existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] BairroRequest request)
    {
        logger.LogInformation("HTTP PUT /api/bairro/{Id}: Atualizando bairro '{Nome}'.", id, request.NomeBairro);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/bairro/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = bairroService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/bairro/{Id}: Bairro não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/bairro/{Id}: Bairro atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove um bairro pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/bairro/{Id}: Excluindo bairro.", id);
        if (!bairroService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/bairro/{Id}: Bairro não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/bairro/{Id}: Bairro excluído com sucesso.", id);
        return NoContent();
    }
}