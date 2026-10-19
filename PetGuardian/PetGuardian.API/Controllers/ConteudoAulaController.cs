using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Conteúdo didático (Markdown) de uma aula, armazenado no MongoDB. Escrita somente Admin.</summary>
[Route("api/aula/{aulaId:guid}/conteudo")]
[ApiController]
[Produces("application/json")]
public class ConteudoAulaController(
    IConteudoAulaService conteudoService,
    ILogger<ConteudoAulaController> logger) : ApiControllerBase
{
    /// <summary>Obtém o conteúdo da aula.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConteudoAulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAulaId(Guid aulaId, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP GET /api/aula/{AulaId}/conteudo: Buscando conteúdo.", aulaId);
        var conteudo = await conteudoService.GetByAulaIdAsync(aulaId, cancellationToken);
        if (conteudo is null)
        {
            logger.LogWarning("HTTP GET /api/aula/{AulaId}/conteudo: Conteúdo não encontrado.", aulaId);
            return NotFound();
        }
        return Ok(ComLinks(conteudo));
    }

    /// <summary>Cadastra o conteúdo da aula (uma aula possui no máximo um conteúdo).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ConteudoAulaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid aulaId, [FromBody] ConteudoAulaRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP POST /api/aula/{AulaId}/conteudo: Cadastrando conteúdo.", aulaId);
        var created = await conteudoService.CreateAsync(aulaId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByAulaId), new { aulaId }, ComLinks(created));
    }

    /// <summary>Atualiza o conteúdo da aula.</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut]
    [ProducesResponseType(typeof(ConteudoAulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid aulaId, [FromBody] ConteudoAulaRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP PUT /api/aula/{AulaId}/conteudo: Atualizando conteúdo.", aulaId);
        var updated = await conteudoService.UpdateAsync(aulaId, request, cancellationToken);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/aula/{AulaId}/conteudo: Conteúdo não encontrado.", aulaId);
            return NotFound();
        }
        return Ok(ComLinks(updated));
    }

    /// <summary>Remove o conteúdo da aula.</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid aulaId, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP DELETE /api/aula/{AulaId}/conteudo: Excluindo conteúdo.", aulaId);
        if (!await conteudoService.DeleteAsync(aulaId, cancellationToken))
        {
            logger.LogWarning("HTTP DELETE /api/aula/{AulaId}/conteudo: Conteúdo não encontrado.", aulaId);
            return NotFound();
        }
        return NoContent();
    }

    private ConteudoAulaResponse ComLinks(ConteudoAulaResponse conteudo) => conteudo with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetByAulaId), new { aulaId = conteudo.AulaId }),
            LinkTo("update", "PUT", nameof(Update), new { aulaId = conteudo.AulaId }),
            LinkTo("delete", "DELETE", nameof(Delete), new { aulaId = conteudo.AulaId }),
            LinkTo("aula", "GET", nameof(AulaController.GetById), new { id = conteudo.AulaId }, "Aula")
        ]
    };
}
