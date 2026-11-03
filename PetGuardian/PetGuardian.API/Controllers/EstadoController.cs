using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Estados da federação. Topo da hierarquia de endereço.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EstadoController(IEstadoService estadoService) : ApiControllerBase
{
    /// <summary>Lista todos os estados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstadoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(estadoService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um estado pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        estadoService.GetById(id) is { } estado ? Ok(ComLinks(estado)) : NotFound();

    /// <summary>Cria um estado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EstadoRequest request)
    {
        var created = estadoService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um estado existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] EstadoRequest request) =>
        estadoService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um estado pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        estadoService.Delete(id) ? NoContent() : NotFound();

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
