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
public class TrilhaController(ITrilhaService trilhaService, ILogger<TrilhaController> logger) : ApiControllerBase
{
    /// <summary>Lista trilhas com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/trilha?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;sortDir=desc&amp;nome=adest</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TrilhaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] TrilhaFilter filter)
    {
        logger.LogInformation("HTTP GET /api/trilha: pesquisando trilhas (página {Page}, tamanho {PageSize}).", query.Page, query.PageSize);
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
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/trilha/{Id}: Buscando trilha.", id);
        var trilha = trilhaService.GetById(id);
        if (trilha is null)
        {
            logger.LogWarning("HTTP GET /api/trilha/{Id}: Trilha não encontrada.", id);
            return NotFound();
        }
        return Ok(ComLinks(trilha));
    }

    /// <summary>Lista trilhas de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrilhaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/trilha/by-pet/{PetId}: Buscando trilhas do pet.", petId);
        return Ok(trilhaService.GetByPetId(petId).Select(ComLinks).ToList());
    }

    /// <summary>Cadastra uma nova trilha na base de dados (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] TrilhaRequest request)
    {
        logger.LogInformation("HTTP POST /api/trilha: Cadastrando trilha '{Nome}' para Pet {PetId}.", request.Nome, request.PetId);
        var created = trilhaService.Create(request);
        logger.LogInformation("HTTP POST /api/trilha: Trilha {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza uma trilha existente (somente Admin; o pet vinculado não é reatribuível por aqui).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrilhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TrilhaUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/trilha/{Id}: Atualizando trilha '{Nome}'.", id, request.Nome);
        var updated = trilhaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/trilha/{Id}: Trilha não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/trilha/{Id}: Trilha atualizada com sucesso.", id);
        return Ok(ComLinks(updated));
    }

    /// <summary>Remove uma trilha pelo Id (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/trilha/{Id}: Excluindo trilha.", id);
        if (!trilhaService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/trilha/{Id}: Trilha não encontrada para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/trilha/{Id}: Trilha excluída com sucesso.", id);
        return NoContent();
    }

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
