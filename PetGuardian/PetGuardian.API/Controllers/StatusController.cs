using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Status de ciclo de vida das tarefas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class StatusController(IStatusService statusService) : ApiControllerBase
{
    /// <summary>Lista todos os registros de status cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StatusResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(statusService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um registro de status pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        statusService.GetById(id) is { } status ? Ok(ComLinks(status)) : NotFound();

    /// <summary>Cadastra um novo registro de status na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] StatusRequest request)
    {
        var created = statusService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um status existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] StatusRequest request) =>
        statusService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Exclui um registro de status cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        statusService.Delete(id) ? NoContent() : NotFound();

    private StatusResponse ComLinks(StatusResponse s) => s with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = s.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = s.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = s.Id }),
            LinkTo("tarefas", "GET", "GetByStatus", new { statusId = s.Id }, "Tarefa")
        ]
    };
}
