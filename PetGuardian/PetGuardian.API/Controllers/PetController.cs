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
    IPetQueryService petQueryService,
    ILogger<PetController> logger) : ApiControllerBase
{
    /// <summary>Lista pets com paginação, ordenação e filtros (resposta com links HATEOAS).</summary>
    /// <remarks>Exemplo: <c>/api/pet?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;sortDir=desc&amp;nome=re&amp;porte=Medio</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] PageQuery page, [FromQuery] PetFilter filter)
    {
        logger.LogInformation("HTTP GET /api/pet: pesquisando pets (página {Page}, tamanho {PageSize}).", page.Page, page.PageSize);
        var resultado = petQueryService.Search(page, filter);

        return Ok(Paginar(resultado, nameof(GetAll), numero => new
        {
            page = numero,
            pageSize = resultado.PageSize,
            sortBy = page.SortBy,
            sortDir = page.SortDir,
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
    public IActionResult GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /api/pet/{Id}: Buscando pet.", id);
        var pet = petService.GetById(id);
        if (pet is null)
        {
            logger.LogWarning("HTTP GET /api/pet/{Id}: Pet não encontrado.", id);
            return NotFound();
        }
        return Ok(ComLinks(pet));
    }

    /// <summary>Lista todos os pets de uma raça.</summary>
    [HttpGet("by-raca/{racaId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByRaca(Guid racaId)
    {
        logger.LogInformation("HTTP GET /api/pet/by-raca/{RacaId}: Buscando pets por raça.", racaId);
        return Ok(petService.GetByRacaId(racaId).Select(ComLinks).ToList());
    }

    /// <summary>Linha do tempo histórica unificada de um pet (Historico + Tarefas concluídas).</summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IReadOnlyList<PetHistoricoItemResponse>), StatusCodes.Status200OK)]
    public IActionResult GetHistorico(Guid id)
    {
        logger.LogInformation("HTTP GET /api/pet/{Id}/historico: Buscando histórico do pet.", id);
        return Ok(petService.GetHistorico(id));
    }

    /// <summary>Cadastra um pet.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] PetRequest request)
    {
        logger.LogInformation("HTTP POST /api/pet: Iniciando cadastro do pet '{Nome}'.", request.Nome);
        var created = petService.Create(request);
        logger.LogInformation("HTTP POST /api/pet: Pet {Id} cadastrado com sucesso.", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ComLinks(created));
    }

    /// <summary>Atualiza um pet existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] PetRequest request)
    {
        logger.LogInformation("HTTP PUT /api/pet/{Id}: Atualizando pet '{Nome}'.", id, request.Nome);
        var updated = petService.Update(id, request);
        if (updated is null)
        {
            logger.LogWarning("HTTP PUT /api/pet/{Id}: Pet não encontrado para atualização.", id);
            return NotFound();
        }
        return Ok(ComLinks(updated));
    }

    /// <summary>Remove um pet pelo Id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /api/pet/{Id}: Excluindo pet.", id);
        if (!petService.Delete(id))
        {
            logger.LogWarning("HTTP DELETE /api/pet/{Id}: Pet não encontrado para exclusão.", id);
            return NotFound();
        }
        return NoContent();
    }

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