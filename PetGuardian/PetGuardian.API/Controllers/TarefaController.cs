using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Tarefas de cuidado, sempre vinculadas a um Pet e a um Usuário responsável.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TarefaController(
    ITarefaService tarefaService,
    IUsuarioPetService usuarioPetService) : ApiControllerBase
{
    /// <summary>Lista tarefas do usuário autenticado ou de seus pets.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyTarefas([FromQuery] bool? concluida = null) =>
        CurrentUserId is { } userId
            ? Ok(tarefaService.GetMinhasTarefas(userId, concluida).Select(ComLinks).ToList())
            : Unauthorized();

    /// <summary>Lista tarefas com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/tarefa?petId={id}&amp;concluida=false&amp;sortBy=Prazo&amp;pageSize=5</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] TarefaFilter filter)
    {
        var resultado = tarefaService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            titulo = filter.Titulo,
            petId = filter.PetId,
            usuarioId = filter.UsuarioId,
            statusId = filter.StatusId,
            concluida = filter.Concluida,
            prazoDe = filter.PrazoDe,
            prazoAte = filter.PrazoAte
        }, ComLinks));
    }

    /// <summary>Obtém uma tarefa pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        tarefaService.GetById(id) is { } t ? Ok(ComLinks(t)) : NotFound();

    /// <summary>Lista tarefas de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId) =>
        Ok(tarefaService.GetByPetId(petId).Select(ComLinks).ToList());

    /// <summary>Lista tarefas de um usuário.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId) =>
        Ok(tarefaService.GetByUsuarioId(usuarioId).Select(ComLinks).ToList());

    /// <summary>Lista tarefas por status.</summary>
    [HttpGet("by-status/{statusId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByStatus(Guid statusId) =>
        Ok(tarefaService.GetByStatusId(statusId).Select(ComLinks).ToList());

    /// <summary>Cadastra uma nova tarefa.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TarefaRequest request)
    {
        var created = tarefaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza título/pontos/descrição/prazo de uma tarefa ainda não concluída.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TarefaUpdateRequest request)
    {
        if (tarefaService.GetById(id) is not { } existing)
            return NotFound();

        if (!CanAccessPetCareCircle(usuarioPetService, existing.PetId))
            return Forbid();

        return Ok(ComLinks(tarefaService.Update(id, request)!));
    }

    /// <summary>Conclui a tarefa em nome de um usuário (o próprio usuário do token ou um Admin).</summary>
    [HttpPost("{id:guid}/concluir")]
    [HttpPatch("{id:guid}/concluir")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Concluir(Guid id, [FromBody] TarefaConcluirRequest request) =>
        IsSelfOrAdmin(request.UsuarioId)
            ? Ok(ComLinks(tarefaService.Concluir(id, request.UsuarioId)))
            : Forbid();

    /// <summary>Reverte a conclusão de uma tarefa, voltando-a para PENDENTE.</summary>
    [HttpPatch("{id:guid}/desmarcar")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Desmarcar(Guid id)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized();

        if (tarefaService.GetById(id) is not { } existing)
            return NotFound();

        return CanAccessPetCareCircle(usuarioPetService, existing.PetId)
            ? Ok(ComLinks(tarefaService.Desmarcar(id, userId)))
            : Forbid();
    }

    /// <summary>Exclui uma tarefa.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (tarefaService.GetById(id) is not { } existing)
            return NotFound();

        if (!CanAccessPetCareCircle(usuarioPetService, existing.PetId))
            return Forbid();

        tarefaService.Delete(id);
        return NoContent();
    }

    private TarefaResponse ComLinks(TarefaResponse t)
    {
        var links = new List<Link>
        {
            LinkTo("self", "GET", nameof(GetById), new { id = t.Id }),
            LinkTo("pet", "GET", "GetById", new { id = t.PetId }, "Pet"),
            LinkTo("usuario", "GET", "GetById", new { id = t.UsuarioId }, "Usuario"),
            LinkTo("status", "GET", "GetById", new { id = t.StatusId }, "Status")
        };

        if (t.Conclusao is null)
        {
            links.Add(LinkTo("update", "PUT", nameof(Update), new { id = t.Id }));
            links.Add(LinkTo("concluir", "POST", nameof(Concluir), new { id = t.Id }));
        }
        else
        {
            links.Add(LinkTo("desmarcar", "PATCH", nameof(Desmarcar), new { id = t.Id }));
        }

        links.Add(LinkTo("delete", "DELETE", nameof(Delete), new { id = t.Id }));
        return t with { Links = links };
    }
}
