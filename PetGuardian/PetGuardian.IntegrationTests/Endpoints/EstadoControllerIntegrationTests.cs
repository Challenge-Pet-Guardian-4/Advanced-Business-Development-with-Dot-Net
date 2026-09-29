using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class EstadoControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var request = new EstadoRequest("Minas Gerais Teste");

        // Act
        var response = await _client.PostAsJsonAsync("/api/estado", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var estado = await response.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(estado);
        Assert.Equal("Minas Gerais Teste", estado.NomeEstado);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/estado", new EstadoRequest("Paraná Teste"));
        var criado = await postRes.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/estado/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/estado/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/estado", new EstadoRequest("Bahia Original"));
        var criado = await postRes.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/estado/{criado.Id}", new EstadoRequest("Bahia Atualizada"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Bahia Atualizada", atualizado.NomeEstado);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/estado", new EstadoRequest("Estado Para Deletar"));
        var criado = await postRes.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/estado/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/estado/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComNomeInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new EstadoRequest("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/estado", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
