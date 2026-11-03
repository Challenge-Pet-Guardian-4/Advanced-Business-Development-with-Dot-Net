using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Telefones de contato. Devem ser criados antes de Usuario.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TelefoneController(ITelefoneService telefoneService) : ApiControllerBase
{
    /// <summary>Lista todos os registros de telefones cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TelefoneResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(telefoneService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um registro de telefone pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        telefoneService.GetById(id) is { } telefone ? Ok(ComLinks(telefone)) : NotFound();

    /// <summary>Cadastra um novo registro de telefone na base de dados.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TelefoneRequest request)
    {
        var created = telefoneService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um telefone existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TelefoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TelefoneRequest request) =>
        telefoneService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Exclui um registro de telefone cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        telefoneService.Delete(id) ? NoContent() : NotFound();

    private TelefoneResponse ComLinks(TelefoneResponse t) => t with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = t.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = t.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = t.Id })
        ]
    };
}
