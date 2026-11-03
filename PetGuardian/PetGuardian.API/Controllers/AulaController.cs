using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.API.Security;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.API.Controllers;

/// <summary>Aulas de um Módulo. Concedem pontos de gamificação ao ser concluídas.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AulaController(IAulaService aulaService) : ApiControllerBase
{
    /// <summary>Lista aulas com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/aula?page=1&amp;pageSize=10&amp;sortBy=PontosAula&amp;sortDir=desc&amp;concluida=false&amp;dificuldade=Facil</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AulaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery query, [FromQuery] AulaFilter filter)
    {
        var resultado = aulaService.Search(query, filter);
        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = query.SortBy,
            sortDir = query.SortDir,
            nome = filter.Nome,
            moduloId = filter.ModuloId,
            dificuldade = filter.Dificuldade,
            concluida = filter.Concluida
        }, ComLinks));
    }

    /// <summary>Obtém uma aula pelo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) =>
        aulaService.GetById(id) is { } aula ? Ok(ComLinks(aula)) : NotFound();

    /// <summary>Lista aulas de um módulo.</summary>
    [HttpGet("by-modulo/{moduloId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AulaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByModulo(Guid moduloId) =>
        Ok(aulaService.GetByModuloId(moduloId).Select(ComLinks).ToList());

    /// <summary>Cadastra uma nova aula na base de dados (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create([FromBody] AulaRequest request)
    {
        var created = aulaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza uma aula existente (somente Admin; o módulo vinculado não é reatribuível por aqui).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] AulaUpdateRequest request) =>
        aulaService.Update(id, request) is { } updated ? Ok(ComLinks(updated)) : NotFound();

    /// <summary>Marca a aula como concluída.</summary>
    [HttpPatch("{id:guid}/concluir")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Concluir(Guid id) =>
        aulaService.Concluir(id) is { } aula ? Ok(ComLinks(aula)) : NotFound();

    /// <summary>Desfaz a conclusão da aula.</summary>
    [HttpPatch("{id:guid}/desmarcar")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Desmarcar(Guid id) =>
        aulaService.Desmarcar(id) is { } aula ? Ok(ComLinks(aula)) : NotFound();

    /// <summary>Remove uma aula pelo Id (somente Admin).</summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await aulaService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private AulaResponse ComLinks(AulaResponse a) => a with
    {
        Links =
        [
            LinkTo("self", "GET", nameof(GetById), new { id = a.Id }),
            LinkTo("update", "PUT", nameof(Update), new { id = a.Id }),
            LinkTo("delete", "DELETE", nameof(Delete), new { id = a.Id }),
            LinkTo(a.Concluida ? "desmarcar" : "concluir", "PATCH",
                a.Concluida ? nameof(Desmarcar) : nameof(Concluir), new { id = a.Id }),
            LinkTo("modulo", "GET", "GetById", new { id = a.ModuloId }, "Modulo"),
            LinkTo("conteudo", "GET", "GetByAulaId", new { aulaId = a.Id }, "ConteudoAula")
        ]
    };
}
