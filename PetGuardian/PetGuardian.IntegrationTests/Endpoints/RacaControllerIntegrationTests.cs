using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class RacaControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var request = new RacaRequest("Border Collie Teste");

        // Act
        var response = await _client.PostAsJsonAsync("/api/raca", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var raca = await response.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(raca);
        Assert.Equal("Border Collie Teste", raca.NomeRaca);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/raca", new RacaRequest("Husky Siberiano Teste"));
        var racaCriada = await postRes.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(racaCriada);

        // Act
        var response = await _client.GetAsync($"/api/raca/{racaCriada.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var racaLida = await response.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(racaLida);
        Assert.Equal(racaCriada.Id, racaLida.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/raca/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/raca", new RacaRequest("Poodle Original"));
        var racaCriada = await postRes.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(racaCriada);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/raca/{racaCriada.Id}", new RacaRequest("Poodle Gigante"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var racaAtualizada = await putRes.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(racaAtualizada);
        Assert.Equal("Poodle Gigante", racaAtualizada.NomeRaca);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/raca", new RacaRequest("Raca Para Deletar"));
        var racaCriada = await postRes.Content.ReadFromJsonAsync<RacaResponse>();
        Assert.NotNull(racaCriada);

        // Act
        var delRes = await _client.DeleteAsync($"/api/raca/{racaCriada.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/raca/{racaCriada.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComNomeInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new RacaRequest("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/raca", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
