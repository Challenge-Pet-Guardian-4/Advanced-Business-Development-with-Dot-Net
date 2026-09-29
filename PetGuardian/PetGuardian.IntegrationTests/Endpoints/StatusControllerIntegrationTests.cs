using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class StatusControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var request = new StatusRequest("PENDENTE");

        // Act
        var response = await _client.PostAsJsonAsync("/api/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var status = await response.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(status);
        Assert.Equal("PENDENTE", status.NomeStatus);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/status", new StatusRequest("CONCLUIDO"));
        var criado = await postRes.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/status/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/status/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/status", new StatusRequest("PENDENTE"));
        var criado = await postRes.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/status/{criado.Id}", new StatusRequest("EXPIRADO"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("EXPIRADO", atualizado.NomeStatus);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/status", new StatusRequest("EXPIRADO"));
        var criado = await postRes.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/status/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/status/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDescricaoInvalida_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new StatusRequest("STATUS_INVENTADO_INVALIDO");

        // Act
        var response = await _client.PostAsJsonAsync("/api/status", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
