using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Raças de pets.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class RacaController(IRacaService racaService) : ApiControllerBase
{
    /// <summary>Lista todos os registros de raças cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RacaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(racaService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um registro de raça pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        racaService.GetById(id) is { } raca ? Ok(ComLinks(raca)) : NotFound();

    /// <summary>Cadastra um novo registro de raça na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] RacaRequest request)
    {
        var created = racaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza uma raça existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RacaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] RacaRequest request) =>
        racaService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Exclui um registro de raça cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        racaService.Delete(id) ? NoContent() : NotFound();

    private RacaResponse ComLinks(RacaResponse r) => r with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = r.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = r.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = r.Id }),
            LinkTo("pets", "GET", "GetByRaca", new { racaId = r.Id }, "Pet")
        ]
    };
}
