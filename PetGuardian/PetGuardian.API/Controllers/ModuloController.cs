using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Módulos de uma Trilha. Agrupam Aulas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ModuloController(IModuloService moduloService) : ApiControllerBase
{
    /// <summary>Lista módulos com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/modulo?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;trilhaId={guid}</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ModuloResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] ModuloFilter filter)
    {
        var resultado = moduloService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            nome = filter.Nome,
            trilhaId = filter.TrilhaId
        }, ComLinks));
    }

    /// <summary>Obtém um módulo pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        moduloService.GetById(id) is { } modulo ? Ok(ComLinks(modulo)) : NotFound();

    /// <summary>Lista módulos de uma trilha.</summary>
    [HttpGet("by-trilha/{trilhaId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuloResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByTrilha(Guid trilhaId) =>
        Ok(moduloService.GetByTrilhaId(trilhaId).Select(ComLinks).ToList());

    /// <summary>Cadastra um novo módulo na base de dados (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] ModuloRequest request)
    {
        var created = moduloService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um módulo existente (somente Admin; a trilha vinculada não é reatribuível por aqui).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ModuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] ModuloUpdateRequest request) =>
        moduloService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um módulo pelo Id (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        moduloService.Delete(id) ? NoContent() : NotFound();

    private ModuloResponse ComLinks(ModuloResponse m) => m with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = m.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = m.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = m.Id }),
            LinkTo("trilha", "GET", "GetById", new { id = m.TrilhaId }, "Trilha"),
            LinkTo("aulas", "GET", "GetByModulo", new { moduloId = m.Id }, "Aula")
        ]
    };
}
