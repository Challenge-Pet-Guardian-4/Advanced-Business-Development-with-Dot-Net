using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Linha do tempo de eventos de um Pet (ex.: tarefas concluídas, marcos de saúde).</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class HistoricoController(IHistoricoService historicoService) : ApiControllerBase
{
    /// <summary>Lista históricos com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<HistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] HistoricoFilter filter)
    {
        var resultado = historicoService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            petId = filter.PetId,
            tipoHist = filter.TipoHist,
            dataDe = filter.DataDe,
            dataAte = filter.DataAte
        }, ComLinks));
    }

    /// <summary>Obtém um registro de histórico pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        historicoService.GetById(id) is { } h ? Ok(ComLinks(h)) : NotFound();

    /// <summary>Lista o histórico de um pet específico.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId) =>
        Ok(historicoService.GetByPetId(petId).Select(ComLinks).ToList());

    /// <summary>Cadastra um novo registro de histórico.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] HistoricoRequest request)
    {
        var created = historicoService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um registro de histórico (o pet vinculado não é reatribuível).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] HistoricoUpdateRequest request) =>
        historicoService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um registro de histórico.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        historicoService.Delete(id) ? NoContent() : NotFound();

    private HistoricoResponse ComLinks(HistoricoResponse h) => h with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = h.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = h.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = h.Id }),
            LinkTo("pet", "GET", "GetById", new { id = h.PetId }, "Pet")
        ]
    };
}
