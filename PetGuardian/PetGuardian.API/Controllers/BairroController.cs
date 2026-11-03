using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Bairros. Pertencem a uma cidade.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class BairroController(IBairroService bairroService) : ApiControllerBase
{
    /// <summary>Lista todos os bairros.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BairroResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(bairroService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um bairro pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        bairroService.GetById(id) is { } bairro ? Ok(ComLinks(bairro)) : NotFound();

    /// <summary>Lista bairros de uma cidade.</summary>
    [HttpGet("by-cidade/{cidadeId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<BairroResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByCidade(Guid cidadeId) =>
        Ok(bairroService.GetByCidadeId(cidadeId).Select(ComLinks).ToList());

    /// <summary>Cria um bairro.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] BairroRequest request)
    {
        var created = bairroService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um bairro existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BairroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] BairroRequest request) =>
        bairroService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um bairro pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        bairroService.Delete(id) ? NoContent() : NotFound();

    private BairroResponse ComLinks(BairroResponse b) => b with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = b.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = b.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = b.Id }),
            LinkTo("cidade", "GET", "GetById", new { id = b.CidadeId }, "Cidade")
        ]
    };
}
