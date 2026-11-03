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
public class ConteudoAulaController(IConteudoAulaService conteudoService) : ApiControllerBase
{
    /// <summary>Obtém o conteúdo da aula.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConteudoAulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAulaId(Guid aulaId, CancellationToken cancellationToken) =>
        await conteudoService.GetByAulaIdAsync(aulaId, cancellationToken) is { } conteudo
            ? Ok(ComLinks(conteudo))
            : NotFound();

    /// <summary>Cadastra o conteúdo da aula (uma aula possui no máximo um conteúdo).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ConteudoAulaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid aulaId, [FromBody] ConteudoAulaRequest request, CancellationToken cancellationToken)
    {
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
        Guid aulaId, [FromBody] ConteudoAulaRequest request, CancellationToken cancellationToken) =>
        await conteudoService.UpdateAsync(aulaId, request, cancellationToken) is { } updated
            ? Ok(ComLinks(updated))
            : NotFound();

    /// <summary>Remove o conteúdo da aula.</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid aulaId, CancellationToken cancellationToken) =>
        await conteudoService.DeleteAsync(aulaId, cancellationToken) ? NoContent() : NotFound();

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
