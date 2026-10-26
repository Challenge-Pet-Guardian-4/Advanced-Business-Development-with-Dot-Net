using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Estados da federação. Topo da hierarquia de endereço.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EstadoController(IEstadoService estadoService, ILogger<EstadoController> logger) : ApiControllerBase
{
    /// <summary>Lista todos os estados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstadoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/estado: Listando todos os estados.");
        return Ok(estadoService.GetAll().Select(ComLinks).ToList());
    }

    /// <summary>Obtém um estado pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/estado/{Id}: Buscando estado.", id);
        var estado = estadoService.GetById(id);
        if (estado is null)
        {
            logger.LogWarning("HTTP GET /api/estado/{Id}: Estado não encontrado.", id);
            return NotFound();
        }
        return Ok(ComLinks(estado));
    }

    /// <summary>Cria um estado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EstadoRequest request)
    {
        logger.LogInformation("HTTP POST /api/estado: Cadastrando estado '{Nome}'.", request.NomeEstado);
        var created = estadoService.Create(request);
        logger.LogInformation("HTTP POST /api/estado: Estado {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um estado existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] EstadoRequest request)
    {
        logger.LogInformation("HTTP PUT /api/estado/{Id}: Atualizando estado '{Nome}'.", id, request.NomeEstado);
        var updated = estadoService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/estado/{Id}: Estado não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/estado/{Id}: Estado atualizado com sucesso.", id);
        return Ok(ComLinks(updated));
    }

    /// <summary>Remove um estado pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/estado/{Id}: Excluindo estado.", id);
        if (!estadoService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/estado/{Id}: Estado não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/estado/{Id}: Estado excluído com sucesso.", id);
        return NoContent();
    }

    private EstadoResponse ComLinks(EstadoResponse e) => e with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = e.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = e.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = e.Id }),
            LinkTo("cidades", "GET", "GetByEstado", new { estadoId = e.Id }, "Cidade")
        ]
    };
}
