using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Gerenciamento de pets no contexto de rede de cuidado.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PetController(
    IPetService petService,
    IUsuarioPetService usuarioPetService) : ApiControllerBase
{
    /// <summary>Lista pets vinculados ao usuário autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyPets()
    {
        if (CurrentUserId is not { } userId) return Unauthorized();
        var petIds = usuarioPetService.GetByUsuarioId(userId).Select(v => v.PetId);
        return Ok(petService.GetByIds(petIds).Select(ComLinks).ToList());
    }

    /// <summary>Lista pets com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/pet?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;sortDir=desc&amp;nome=re&amp;porte=Medio</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] PetFilter filter)
    {
        var resultado = petService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            nome = filter.Nome,
            racaId = filter.RacaId,
            porte = filter.Porte,
            sexo = filter.Sexo,
            castrado = filter.Castrado
        }, ComLinks));
    }

    /// <summary>Obtém um pet pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        petService.GetById(id) is { } pet ? Ok(ComLinks(pet)) : NotFound();

    /// <summary>Lista todos os pets de uma raça.</summary>
    [HttpGet("by-raca/{racaId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByRaca(Guid racaId) =>
        Ok(petService.GetByRacaId(racaId).Select(ComLinks).ToList());

    /// <summary>Linha do tempo histórica unificada de um pet (Historico + Tarefas concluídas).</summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IReadOnlyList<PetHistoricoItemResponse>), StatusCodes.Status200OK)]
    public IActionResult GetHistorico(Guid id) => Ok(petService.GetHistorico(id));

    /// <summary>Cadastra um pet.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] PetRequest request)
    {
        var created = petService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um pet existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] PetRequest request) =>
        petService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Remove um pet pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        petService.Delete(id) ? NoContent() : NotFound();

    private PetResponse ComLinks(PetResponse p) => p with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = p.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = p.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = p.Id }),
            LinkTo("historico", "GET", nameof(GetHistorico), new { id = p.Id }),
            LinkTo("tarefas", "GET", "GetByPet", new { petId = p.Id }, "Tarefa"),
            LinkTo("trilhas", "GET", "GetByPet", new { petId = p.Id }, "Trilha"),
            LinkTo("raca", "GET", "GetById", new { id = p.RacaId }, "Raca")
        ]
    };
}
