using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Endereços. Devem ser criados após Bairro. Usados por Usuario.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EnderecoController(IEnderecoService enderecoService, ILogger<EnderecoController> logger) : ControllerBase
{
    /// <summary>Lista todos os endereços.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/endereco: Listando todos os endereços.");
        return Ok(enderecoService.GetAll());
    }

    /// <summary>Obtém um endereço pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/endereco/{Id}: Buscando endereço.", id);
        var endereco = enderecoService.GetById(id);
        if (endereco is null)
        {
            logger.LogWarning("HTTP GET /api/endereco/{Id}: Endereço não encontrado.", id);
            return NotFound();
        }
        return Ok(endereco);
    }

    /// <summary>Cria um endereço.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EnderecoRequest request)
    {
        logger.LogInformation("HTTP POST /api/endereco: Cadastrando endereço com CEP {Cep}.", request.Cep);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/endereco: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = enderecoService.Create(request);
        logger.LogInformation("HTTP POST /api/endereco: Endereço {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um endereço existente (o CEP é reconsultado no ViaCEP).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] EnderecoRequest request)
    {
        logger.LogInformation("HTTP PUT /api/endereco/{Id}: Atualizando endereço com CEP {Cep}.", id, request.Cep);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/endereco/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = enderecoService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/endereco/{Id}: Endereço não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/endereco/{Id}: Endereço atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Remove um endereço pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/endereco/{Id}: Excluindo endereço.", id);
        if (!enderecoService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/endereco/{Id}: Endereço não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/endereco/{Id}: Endereço excluído com sucesso.", id);
        return NoContent();
    }
}