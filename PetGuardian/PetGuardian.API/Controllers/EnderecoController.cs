using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Endereços de contato. Devem ser criados antes de Usuario.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EnderecoController(IEnderecoService enderecoService, ILogger<EnderecoController> logger) : ApiControllerBase
{
    /// <summary>Lista todos os endereços.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/endereco: Listando todos os endereços.");
        return Ok(enderecoService.GetAll().Select(ComLinks).ToList());
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
        return Ok(ComLinks(endereco));
    }

    /// <summary>Cria um endereço.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EnderecoRequest request)
    {
        logger.LogInformation("HTTP POST /api/endereco: Cadastrando endereço com CEP {Cep}.", request.Cep);
        var created = enderecoService.Create(request);
        logger.LogInformation("HTTP POST /api/endereco: Endereço {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um endereço existente (o CEP é reconsultado no ViaCEP).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] EnderecoRequest request)
    {
        logger.LogInformation("HTTP PUT /api/endereco/{Id}: Atualizando endereço com CEP {Cep}.", id, request.Cep);
        var updated = enderecoService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/endereco/{Id}: Endereço não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/endereco/{Id}: Endereço atualizado com sucesso.", id);
        return Ok(ComLinks(updated));
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

    private EnderecoResponse ComLinks(EnderecoResponse e) => e with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = e.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = e.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = e.Id }),
            LinkTo("bairro", "GET", "GetById", new { id = e.BairroId }, "Bairro")
        ]
    };
}
