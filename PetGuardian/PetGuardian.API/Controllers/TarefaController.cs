using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>
/// Tarefas de cuidado. SPRINT 3: sempre vinculadas a um Pet e a um Usuario responsável
/// (Veterinario, que antes era obrigatório, não existe mais).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TarefaController(ITarefaService tarefaService, ILogger<TarefaController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de tarefas de cuidado cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/tarefa: Listando todas as tarefas.");
        return Ok(tarefaService.GetAll());
    }

    /// <summary>Obtém um registro de tarefa de cuidado pelo seu identificador único (ID).</summary>
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
        return Ok(t);
    }

    /// <summary>Lista todos os registros de tarefas de cuidado associados a um pet específico.</summary>
    [HttpGet("by-pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByPet(Guid petId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-pet/{PetId}: Buscando tarefas por pet.", petId);
        return Ok(tarefaService.GetByPetId(petId));
    }

    /// <summary>Lista todos os registros de tarefas de cuidado associados a um usuário específico.</summary>
    [HttpGet("by-usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByUsuario(Guid usuarioId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-usuario/{UsuarioId}: Buscando tarefas por usuário.", usuarioId);
        return Ok(tarefaService.GetByUsuarioId(usuarioId));
    }

    /// <summary>Lista todos os registros de tarefas de cuidado associados a um status específico.</summary>
    [HttpGet("by-status/{statusId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TarefaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByStatus(Guid statusId)
    {
        logger.LogInformation("HTTP GET /api/tarefa/by-status/{StatusId}: Buscando tarefas por status.", statusId);
        return Ok(tarefaService.GetByStatusId(statusId));
    }

    /// <summary>Cadastra um novo registro de tarefa de cuidado na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TarefaRequest request)
    {
        logger.LogInformation("HTTP POST /api/tarefa: Cadastrando tarefa '{Titulo}' para Pet {PetId}.", request.Titulo, request.PetId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/tarefa: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = tarefaService.Create(request);
        logger.LogInformation("HTTP POST /api/tarefa: Tarefa {Id} cadastrada com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza título/pontos/descrição/prazo de uma tarefa ainda não concluída.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] TarefaUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/tarefa/{Id}: Atualizando tarefa '{Titulo}'.", id, request.Titulo);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/tarefa/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = tarefaService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/tarefa/{Id}: Tarefa não encontrada para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/tarefa/{Id}: Tarefa atualizada com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Registra a conclusão de uma tarefa de cuidado por um usuário, somando os pontos ao seu score.</summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Concluir(Guid id, [FromBody] TarefaConcluirRequest request)
    {
        logger.LogInformation("HTTP POST /api/tarefa/{Id}/concluir: Concluindo tarefa pelo Usuário {UsuarioId}.", id, request.UsuarioId);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/tarefa/{Id}/concluir: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var res = tarefaService.Concluir(id, request.UsuarioId);
        logger.LogInformation("HTTP POST /api/tarefa/{Id}/concluir: Tarefa concluída com sucesso.", id);
        return Ok(res);
    }

    /// <summary>Exclui um registro de tarefa de cuidado cadastrado pelo seu ID.</summary>
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
        logger.LogInformation("HTTP DELETE /api/tarefa/{Id}: Tarefa excluída com sucesso.", id);
        return NoContent();
    }
}
