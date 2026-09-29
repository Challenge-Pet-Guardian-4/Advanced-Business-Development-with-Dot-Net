using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Usuários. Senha nunca é exposta nas respostas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger) : ControllerBase
{
    /// <summary>Lista todos os registros de usuários cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        logger.LogInformation("HTTP GET /api/usuario: Listando todos os usuários.");
        return Ok(usuarioService.GetAll());
    }

    /// <summary>Obtém um registro de usuário pelo seu identificador único (ID).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/usuario/{Id}: Buscando usuário.", id);
        var u = usuarioService.GetById(id);
        if (u is null)
        {
            logger.LogWarning("HTTP GET /api/usuario/{Id}: Usuário não encontrado.", id);
            return NotFound();
        }
        return Ok(u);
    }

    /// <summary>Obtém um registro de usuário buscando pelo e-mail informado.</summary>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByEmail([FromQuery] string email)
    {
        logger.LogInformation("HTTP GET /api/usuario/by-email: Buscando usuário por e-mail {Email}.", email);
        var u = usuarioService.GetByEmail(email);
        if (u is null)
        {
            logger.LogWarning("HTTP GET /api/usuario/by-email: Usuário com e-mail {Email} não encontrado.", email);
            return NotFound();
        }
        return Ok(u);
    }

    /// <summary>Retorna o score cumulativo e as tarefas concluídas de um usuário.</summary>
    [HttpGet("{id:guid}/score")]
    [ProducesResponseType(typeof(UsuarioScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetScore(Guid id)
    {
        logger.LogInformation("HTTP GET /api/usuario/{Id}/score: Buscando score do usuário.", id);
        return Ok(usuarioService.GetScore(id));
    }

    /// <summary>Cadastra um novo registro de usuário na base de dados.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] UsuarioRequest request)
    {
        logger.LogInformation("HTTP POST /api/usuario: Iniciando cadastro do usuário '{Nome}' ({Email}).", request.Nome, request.Email);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP POST /api/usuario: ModelState inválido.");
            return BadRequest(ModelState);
        }
        var created = usuarioService.Create(request);
        logger.LogInformation("HTTP POST /api/usuario: Usuário {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza nome/e-mail/senha de um usuário (telefone não é reatribuível por aqui).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UsuarioUpdateRequest request)
    {
        logger.LogInformation("HTTP PUT /api/usuario/{Id}: Atualizando usuário '{Nome}'.", id, request.Nome);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("HTTP PUT /api/usuario/{Id}: ModelState inválido.", id);
            return BadRequest(ModelState);
        }
        var updated = usuarioService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/usuario/{Id}: Usuário não encontrado para atualização.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP PUT /api/usuario/{Id}: Usuário atualizado com sucesso.", id);
        return Ok(updated);
    }

    /// <summary>Exclui um registro de usuário cadastrado pelo seu ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/usuario/{Id}: Excluindo usuário.", id);
        if (!usuarioService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/usuario/{Id}: Usuário não encontrado para exclusão.", id);
            return NotFound();
        }
        logger.LogInformation("HTTP DELETE /api/usuario/{Id}: Usuário excluído com sucesso.", id);
        return NoContent();
    }
}