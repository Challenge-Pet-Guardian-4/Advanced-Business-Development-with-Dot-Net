using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Catálogo de trilhas educativas armazenado no MongoDB (documentos com módulos e aulas embutidos).</summary>
[Route("api/catalogo/trilhas")]
[ApiController]
[Produces("application/json")]
public class TrilhaCatalogoController(
    ITrilhaCatalogoService catalogoService,
    ILogger<TrilhaCatalogoController> logger) : ApiControllerBase
{
    /// <summary>Busca trilhas no MongoDB com paginação, ordenação (nome, descricao, sincronizadoEm, petNome) e filtros.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TrilhaCatalogoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PageQuery page, [FromQuery] TrilhaCatalogoFilter filter, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP GET /api/catalogo/trilhas: pesquisando catálogo (página {Page}).", page.Page);
        var resultado = await catalogoService.BuscarAsync(page, filter, cancellationToken);

        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = page.SortBy,
            sortDir = page.SortDir,
            termo = filter.Termo,
            petId = filter.PetId,
            dificuldade = filter.Dificuldade
        }, ComLinks));
    }

    /// <summary>Obtém o documento da trilha (id da trilha no relacional).</summary>
    [HttpGet("{trilhaId:guid}")]
    [ProducesResponseType(typeof(TrilhaCatalogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTrilhaId(Guid trilhaId, CancellationToken cancellationToken)
    {
        var trilha = await catalogoService.GetByTrilhaIdAsync(trilhaId, cancellationToken);
        if (trilha is null)
        {
            logger.LogWarning("HTTP GET /api/catalogo/trilhas/{TrilhaId}: trilha não sincronizada no MongoDB.", trilhaId);
            return NotFound();
        }
        return Ok(ComLinks(trilha));
    }

    /// <summary>Exporta as trilhas do Oracle para o MongoDB (idempotente). Somente Admin.</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("sincronizar")]
    [ProducesResponseType(typeof(SincronizacaoCatalogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Sincronizar(CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP POST /api/catalogo/trilhas/sincronizar: iniciando sincronização.");
        return Ok(await catalogoService.SincronizarAsync(cancellationToken));
    }

    private TrilhaCatalogoResponse ComLinks(TrilhaCatalogoResponse t) => t with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetByTrilhaId), new { trilhaId = t.TrilhaIdOrigem }),
            LinkTo("colecao", "GET", nameof(GetAll)),
            LinkTo("trilha-relacional", "GET", "GetById", new { id = t.TrilhaIdOrigem }, "Trilha"),
            LinkTo("pet", "GET", "GetById", new { id = t.PetAlvo.PetId }, "Pet")
        ]
    };
}