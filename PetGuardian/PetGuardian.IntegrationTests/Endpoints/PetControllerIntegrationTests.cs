using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class PetControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> ObterRacaIdAsync()
    {
        var racasResponse = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racasResponse);
        Assert.NotEmpty(racasResponse);
        return racasResponse.First().Id;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var racaId = await ObterRacaIdAsync();
        var request = new PetRequest("Rex Teste", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, racaId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/pet", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var petCriado = await response.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(petCriado);
        Assert.Equal("Rex Teste", petCriado.Nome);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var racaId = await ObterRacaIdAsync();
        var postRes = await _client.PostAsJsonAsync("/api/pet", new PetRequest("Bob Teste", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Pequeno, false, racaId));
        var criado = await postRes.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/pet/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/pet/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var racaId = await ObterRacaIdAsync();
        var postRes = await _client.PostAsJsonAsync("/api/pet", new PetRequest("Mel Original", DateTime.UtcNow.AddYears(-3), SexoPet.Femea, PortePet.Pequeno, false, racaId));
        var criado = await postRes.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(criado);

        // Act
        var updateRequest = new PetRequest("Mel Atualizada", DateTime.UtcNow.AddYears(-3), SexoPet.Femea, PortePet.Pequeno, true, racaId);
        var putRes = await _client.PutAsJsonAsync($"/api/pet/{criado.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Mel Atualizada", atualizado.Nome);
        Assert.True(atualizado.Castrado);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var racaId = await ObterRacaIdAsync();
        var postRes = await _client.PostAsJsonAsync("/api/pet", new PetRequest("Pet Para Deletar", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Grande, false, racaId));
        var criado = await postRes.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/pet/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/pet/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new PetRequest("", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/pet", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
