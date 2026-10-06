using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class PaginacaoHateoasIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    private async Task<string> CriarTresPetsAsync()
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        var racaId = racas!.First().Id;
        var prefixo = $"Pg{Guid.NewGuid():N}"[..10];

        foreach (var sufixo in new[] { "A", "B", "C" })
        {
            var res = await _client.PostAsJsonAsync("/api/pet",
                new PetRequest(prefixo + sufixo, DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, racaId));
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        return prefixo;
    }

    [Fact]
    public async Task GetPets_PaginacaoEOrdenacaoDesc_DeveRetornarPaginaComTotaisELinks()
    {
        // Arrange
        var prefixo = await CriarTresPetsAsync();

        // Act
        var response = await _client.GetAsync($"/api/pet?nome={prefixo}&page=1&pageSize=2&sortBy=Nome&sortDir=desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<PetResponse>>();
        Assert.NotNull(pagina);
        Assert.Equal(2, pagina.Items.Count);
        Assert.Equal(3, pagina.TotalCount);
        Assert.Equal(2, pagina.TotalPages);
        Assert.EndsWith("C", pagina.Items[0].Nome);
        Assert.Contains(pagina.Links!, l => l.Rel == "next");
        Assert.DoesNotContain(pagina.Links!, l => l.Rel == "prev");
        Assert.All(pagina.Items, i => Assert.Contains(i.Links!, l => l.Rel == "self"));
    }

    [Fact]
    public async Task GetPets_SeguirLinkNext_DeveRetornarSegundaPagina()
    {
        // Arrange
        var prefixo = await CriarTresPetsAsync();
        var primeira = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/pet?nome={prefixo}&pageSize=2&sortBy=Nome");
        var next = primeira!.Links!.Single(l => l.Rel == "next");

        // Act
        var segunda = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(next.Href);

        // Assert
        Assert.NotNull(segunda);
        Assert.Equal(2, segunda.Page);
        Assert.Single(segunda.Items);
        Assert.Contains(segunda.Links!, l => l.Rel == "prev");
    }

    [Fact]
    public async Task GetPets_FiltroPorNome_DeveRetornarSomenteCorrespondentes()
    {
        // Arrange
        var prefixo = await CriarTresPetsAsync();

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>($"/api/pet?nome={prefixo}B");

        // Assert
        Assert.Single(pagina!.Items);
        Assert.EndsWith("B", pagina.Items[0].Nome);
    }

    [Fact]
    public async Task GetPets_SortByInvalido_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/pet?sortBy=CampoQueNaoExiste");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsuarios_SortBySenha_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/usuario?sortBy=Senha");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPets_PageSizeAcimaDoLimite_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/pet?pageSize=1000");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPetPorId_IdExistente_DeveRetornarLinksHateoas()
    {
        // Arrange
        var prefixo = await CriarTresPetsAsync();
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>($"/api/pet?nome={prefixo}A");
        var id = pagina!.Items[0].Id;

        // Act
        var pet = await _client.GetFromJsonAsync<PetResponse>($"/api/pet/{id}");

        // Assert
        Assert.NotNull(pet?.Links);
        var rels = pet.Links.Select(l => l.Rel).ToList();
        Assert.Contains("self", rels);
        Assert.Contains("update", rels);
        Assert.Contains("delete", rels);
        Assert.Contains("historico", rels);
        Assert.Contains("tarefas", rels);
    }
}