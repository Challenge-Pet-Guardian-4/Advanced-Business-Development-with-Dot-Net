using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class CidadeControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<EstadoResponse> CriarEstadoAuxiliarAsync(string nome)
    {
        var res = await _client.PostAsJsonAsync("/api/estado", new EstadoRequest(nome));
        var estado = await res.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(estado);
        return estado;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var estado = await CriarEstadoAuxiliarAsync("Estado Para Cidade Post");
        var request = new CidadeRequest("Sorocaba Teste", estado.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/cidade", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var cidade = await response.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(cidade);
        Assert.Equal("Sorocaba Teste", cidade.NomeCidade);
        Assert.Equal(estado.Id, cidade.EstadoId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var estado = await CriarEstadoAuxiliarAsync("Estado Para Cidade Get");
        var postRes = await _client.PostAsJsonAsync("/api/cidade", new CidadeRequest("Santos Teste", estado.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/cidade/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/cidade/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var estado = await CriarEstadoAuxiliarAsync("Estado Para Cidade Put");
        var postRes = await _client.PostAsJsonAsync("/api/cidade", new CidadeRequest("Ribeirão Preto Original", estado.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/cidade/{criado.Id}", new CidadeRequest("Ribeirão Preto Atualizado", estado.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Ribeirão Preto Atualizado", atualizado.NomeCidade);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var estado = await CriarEstadoAuxiliarAsync("Estado Para Cidade Del");
        var postRes = await _client.PostAsJsonAsync("/api/cidade", new CidadeRequest("Cidade Para Deletar", estado.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/cidade/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/cidade/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new CidadeRequest("", Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/cidade", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
