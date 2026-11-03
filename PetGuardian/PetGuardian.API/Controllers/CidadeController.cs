using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Cidades. Pertencem a um estado.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CidadeController(ICidadeService cidadeService) : ApiControllerBase
{
    /// <summary>Lista todas as cidades.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CidadeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(cidadeService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém uma cidade pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        cidadeService.GetById(id) is { } cidade ? Ok(ComLinks(cidade)) : NotFound();

    /// <summary>Lista cidades de um estado.</summary>
    [HttpGet("by-estado/{estadoId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CidadeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByEstado(Guid estadoId) =>
        Ok(cidadeService.GetByEstadoId(estadoId).Select(ComLinks).ToList());

    /// <summary>Cria uma cidade.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CidadeRequest request)
    {
        var created = cidadeService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza uma cidade existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] CidadeRequest request) =>
        cidadeService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove uma cidade pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        cidadeService.Delete(id) ? NoContent() : NotFound();

    private CidadeResponse ComLinks(CidadeResponse c) => c with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = c.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = c.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = c.Id }),
            LinkTo("estado", "GET", "GetById", new { id = c.EstadoId }, "Estado"),
            LinkTo("bairros", "GET", "GetByCidade", new { cidadeId = c.Id }, "Bairro")
        ]
    };
}
