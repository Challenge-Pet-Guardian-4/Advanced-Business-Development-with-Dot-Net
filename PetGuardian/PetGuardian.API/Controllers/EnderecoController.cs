using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Endereços de contato. Devem ser criados antes de Usuario.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EnderecoController(IEnderecoService enderecoService) : ApiControllerBase
{
    /// <summary>Lista todos os endereços.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnderecoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(enderecoService.GetAll().Select(ComLinks).ToList());

    /// <summary>Obtém um endereço pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        enderecoService.GetById(id) is { } endereco ? Ok(ComLinks(endereco)) : NotFound();

    /// <summary>Cria um endereço.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] EnderecoRequest request)
    {
        var created = enderecoService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um endereço existente (o CEP é reconsultado no ViaCEP).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] EnderecoRequest request) =>
        enderecoService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um endereço pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        enderecoService.Delete(id) ? NoContent() : NotFound();

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
