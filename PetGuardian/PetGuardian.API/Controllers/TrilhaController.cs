using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Trilhas de cuidado/aprendizado vinculadas a um Pet. Agrupam Módulos.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TrilhaController(
    ITrilhaService trilhaService,
    IUsuarioPetService usuarioPetService) : ApiControllerBase
{
    /// <summary>Lista trilhas dos pets do usuário autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<TrilhaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyTrilhas()
    {
        if (CurrentUserId is not { } userId) return Unauthorized();
        var petIds = usuarioPetService.GetByUsuarioId(userId).Select(v => v.PetId);
        return Ok(trilhaService.GetByPetIds(petIds).Select(ComLinks).ToList());
    }

    /// <summary>Lista trilhas com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/trilha?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;sortDir=desc&amp;nome=adest</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TrilhaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] TrilhaFilter filter)
    {
        var resultado = trilhaService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            nome = filter.Nome,
            petId = filter.PetId
        }, ComLinks));
    }

    /// <summary>Obtém uma trilha pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        trilhaService.GetById(id) is { } trilha ? Ok(ComLinks(trilha)) : NotFound();

    /// <summary>Lista trilhas de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrilhaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId) =>
        Ok(trilhaService.GetByPetId(petId).Select(ComLinks).ToList());

    /// <summary>Cadastra uma nova trilha na base de dados (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] TrilhaRequest request)
    {
        var created = trilhaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza uma trilha existente (somente Admin; o pet vinculado não é reatribuível por aqui).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TrilhaUpdateRequest request) =>
        trilhaService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove uma trilha pelo Id (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        trilhaService.Delete(id) ? NoContent() : NotFound();

    private TrilhaResponse ComLinks(TrilhaResponse t) => t with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = t.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = t.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = t.Id }),
            LinkTo("modulos", "GET", "GetByTrilha", new { trilhaId = t.Id }, "Modulo"),
            LinkTo("pet", "GET", "GetById", new { id = t.PetId }, "Pet")
        ]
    };
}
