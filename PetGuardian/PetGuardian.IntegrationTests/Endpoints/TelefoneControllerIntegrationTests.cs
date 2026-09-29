using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class TelefoneControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var request = new TelefoneRequest("11", "987654321");

        // Act
        var response = await _client.PostAsJsonAsync("/api/telefone", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var telefone = await response.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(telefone);
        Assert.Equal("11", telefone.NumDdd);
        Assert.Equal("987654321", telefone.NumTel);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("21", "912345678"));
        var criado = await postRes.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/telefone/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/telefone/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("19", "955551111"));
        var criado = await postRes.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/telefone/{criado.Id}", new TelefoneRequest("19", "999992222"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("999992222", atualizado.NumTel);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "944443333"));
        var criado = await postRes.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/telefone/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/telefone/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComTelefoneInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new TelefoneRequest("1", "123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/telefone", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
