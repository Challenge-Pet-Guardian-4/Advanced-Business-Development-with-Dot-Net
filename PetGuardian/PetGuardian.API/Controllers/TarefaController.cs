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
    ITarefaQueryService tarefaQueryService,
    ILogger<TarefaController> logger) : ApiControllerBase
{
    /// <summary>Lista tarefas com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/tarefa?petId={id}&amp;concluida=false&amp;sortBy=Prazo&amp;pageSize=5</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery page, [FromQuery] TarefaFilter filter)
    {
        logger.LogInformation("HTTP GET /api/tarefa: pesquisando tarefas (página {Page}, tamanho {PageSize}).", page.Page, page.PageSize);
        var resultado = tarefaQueryService.Search(page, filter);

        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = page.SortBy,
            sortDir = page.SortDir,
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
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/tarefa/{Id}: Buscando tarefa.", id);
        var t = tarefaService.GetById(id);
        if (t is null)
        {
            logger.LogWarning("HTTP GET /api/tarefa/{Id}: Tarefa não encontrada.", id);
            return NotFound();
        }
        return Ok(ComLinks(t));
    }

    /// <summary>Lista tarefas de um pet.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-pet/{PetId}: Buscando tarefas por pet.", petId);
        return Ok(tarefaService.GetByPetId(petId).Select(ComLinks).ToList());
    }

    /// <summary>Lista tarefas de um usuário.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-usuario/{UsuarioId}: Buscando tarefas por usuário.", usuarioId);
        return Ok(tarefaService.GetByUsuarioId(usuarioId).Select(ComLinks).ToList());
    }

    /// <summary>Lista tarefas por status.</summary>
    [HttpGet("by-status/{statusId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByStatus(Guid statusId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-status/{StatusId}: Buscando tarefas por status.", statusId);
        return Ok(tarefaService.GetByStatusId(statusId).Select(ComLinks).ToList());
    }

    /// <summary>Cadastra uma nova tarefa.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TarefaRequest request)
    {
        logger.LogInformation("HTTP POST /api/tarefa: Cadastrando tarefa '{Titulo}' para Pet {PetId}.", request.Titulo, request.PetId);
        var created = tarefaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza título/pontos/descrição/prazo de uma tarefa ainda não concluída.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TarefaUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/tarefa/{Id}: Atualizando tarefa '{Titulo}'.", id, request.Titulo);
        var updated = tarefaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/tarefa/{Id}: Tarefa não encontrada para atualização.", id);
            return NotFound();
        }
        return Ok(ComLinks(updated));
    }

    /// <summary>Conclui a tarefa em nome de um usuário (o próprio usuário do token ou um Admin).</summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Concluir(Guid id, [FromBody] TarefaConcluirRequest request)
    {
        if (!IsSelfOrAdmin(request.UsuarioId))
        {
            logger.LogWarning("HTTP POST /api/tarefa/{Id}/concluir: usuário do token não pode concluir em nome de {UsuarioId}.", id, request.UsuarioId);
            return Forbid();
        }

        logger.LogInformation("HTTP POST /api/tarefa/{Id}/concluir: Concluindo tarefa pelo Usuário {UsuarioId}.", id, request.UsuarioId);
        return Ok(ComLinks(tarefaService.Concluir(id, request.UsuarioId)));
    }

    /// <summary>Exclui uma tarefa.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/tarefa/{Id}: Excluindo tarefa.", id);
        if (!tarefaService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/tarefa/{Id}: Tarefa não encontrada para exclusão.", id);
            return NotFound();
        }
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

        links.Add(LinkTo("delete", "DELETE", nameof(Delete), new { id = t.Id }));
        return t with { Links = links };
    }
}