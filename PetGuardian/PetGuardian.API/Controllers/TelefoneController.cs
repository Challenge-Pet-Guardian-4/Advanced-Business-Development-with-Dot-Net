using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Telefones de contato. Devem ser criados antes de Usuario.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TelefoneController(ITelefoneService telefoneService, ILogger<TelefoneController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de telefones cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TelefoneResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/telefone: Listando todos os telefones.");
        return Ok(telefoneService.GetAll());
    }

    /// <summary>Obtém um registro de telefone pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/telefone/{Id}: Buscando telefone.", id);
        var telefone = telefoneService.GetById(id);
        if (telefone is null)
        {
            logger.LogWarning("HTTP GET /api/telefone/{Id}: Telefone não encontrado.", id);
            return NotFound();
        }
        return Ok(telefone);
    }

    /// <summary>Cadastra um novo registro de telefone na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TelefoneRequest request)
    {
        logger.LogInformation("HTTP POST /api/telefone: Cadastrando telefone ({DDD}) {Numero}.", request.NumDdd, request.NumTel);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/telefone: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = telefoneService.Create(request);
        logger.LogInformation("HTTP POST /api/telefone: Telefone {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um telefone existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TelefoneRequest request)
    {
        logger.LogInformation("HTTP PUT /api/telefone/{Id}: Atualizando telefone ({DDD}) {Numero}.", id, request.NumDdd, request.NumTel);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/telefone/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = telefoneService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/telefone/{Id}: Telefone não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/telefone/{Id}: Telefone atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Exclui um registro de telefone cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/telefone/{Id}: Excluindo telefone.", id);
        if (!telefoneService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/telefone/{Id}: Telefone não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/telefone/{Id}: Telefone excluído com sucesso.", id);
        return NoContent();
    }
}