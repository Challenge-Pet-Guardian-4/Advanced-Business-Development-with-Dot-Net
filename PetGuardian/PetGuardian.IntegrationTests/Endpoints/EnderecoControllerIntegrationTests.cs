using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class EnderecoControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ComViaCepValido_DeveRetornar201CreatedEResolverLocalidade()
    {
        // Arrange
        var request = new EnderecoRequest("01001-000", "50");

        // Act
        var response = await _client.PostAsJsonAsync("/api/endereco", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var endereco = await response.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(endereco);
        Assert.Equal("01001000", endereco.Cep);
        Assert.Equal("50", endereco.Numero);
        Assert.Equal("Praça da Sé", endereco.Rua);
        Assert.NotEqual(Guid.Empty, endereco.BairroId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/endereco", new EnderecoRequest("01001-000", "120"));
        var criado = await postRes.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/endereco/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/endereco/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/endereco", new EnderecoRequest("01001-000", "50"));
        var criado = await postRes.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/endereco/{criado.Id}", new EnderecoRequest("01001-000", "55"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("55", atualizado.Numero);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var postRes = await _client.PostAsJsonAsync("/api/endereco", new EnderecoRequest("01001-000", "999"));
        var criado = await postRes.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/endereco/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/endereco/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComCepInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new EnderecoRequest("99999-999", "10");

        // Act
        var response = await _client.PostAsJsonAsync("/api/endereco", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
