using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class BairroControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<CidadeResponse> CriarCidadeAuxiliarAsync(string nome)
    {
        var estRes = await _client.PostAsJsonAsync("/api/estado", new EstadoRequest("Estado Para Bairro"));
        var estado = await estRes.Content.ReadFromJsonAsync<EstadoResponse>();
        Assert.NotNull(estado);

        var cidRes = await _client.PostAsJsonAsync("/api/cidade", new CidadeRequest(nome, estado.Id));
        var cidade = await cidRes.Content.ReadFromJsonAsync<CidadeResponse>();
        Assert.NotNull(cidade);
        return cidade;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var cidade = await CriarCidadeAuxiliarAsync("Cidade Para Bairro Post");
        var request = new BairroRequest("Vila Madalena Teste", cidade.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/bairro", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var bairro = await response.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(bairro);
        Assert.Equal("Vila Madalena Teste", bairro.NomeBairro);
        Assert.Equal(cidade.Id, bairro.CidadeId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var cidade = await CriarCidadeAuxiliarAsync("Cidade Para Bairro Get");
        var postRes = await _client.PostAsJsonAsync("/api/bairro", new BairroRequest("Moema Teste", cidade.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/bairro/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/bairro/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var cidade = await CriarCidadeAuxiliarAsync("Cidade Para Bairro Put");
        var postRes = await _client.PostAsJsonAsync("/api/bairro", new BairroRequest("Pinheiros Original", cidade.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/bairro/{criado.Id}", new BairroRequest("Pinheiros Atualizado", cidade.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Pinheiros Atualizado", atualizado.NomeBairro);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var cidade = await CriarCidadeAuxiliarAsync("Cidade Para Bairro Del");
        var postRes = await _client.PostAsJsonAsync("/api/bairro", new BairroRequest("Bairro Para Deletar", cidade.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<BairroResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/bairro/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/bairro/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new BairroRequest("", Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/bairro", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
