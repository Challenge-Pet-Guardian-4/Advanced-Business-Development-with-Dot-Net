using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ModuloControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<TrilhaResponse> CriarTrilhaAuxiliarAsync(string nome)
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            "Pet Modulo",
            DateTime.UtcNow.AddYears(-1),
            SexoPet.Macho,
            PortePet.Medio,
            false,
            racas.First().Id
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);

        var trilhaPost = await _client.PostAsJsonAsync("/api/trilha", new TrilhaRequest(nome, "Desc Trilha", pet.Id));
        var trilha = await trilhaPost.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(trilha);
        return trilha;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var trilha = await CriarTrilhaAuxiliarAsync("Trilha Para Modulo Post");
        var request = new ModuloRequest("Módulo 1: Introdução", "1 hora", "Conteúdo introdutório", trilha.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/modulo", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var modulo = await response.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(modulo);
        Assert.Equal("Módulo 1: Introdução", modulo.Nome);
        Assert.Equal(trilha.Id, modulo.TrilhaId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var trilha = await CriarTrilhaAuxiliarAsync("Trilha Para Modulo Get");
        var postRes = await _client.PostAsJsonAsync("/api/modulo", new ModuloRequest("Módulo Prático", "30 min", "Desc", trilha.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/modulo/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/modulo/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var trilha = await CriarTrilhaAuxiliarAsync("Trilha Para Modulo Put");
        var postRes = await _client.PostAsJsonAsync("/api/modulo", new ModuloRequest("Módulo Original", "45 min", "Desc", trilha.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/modulo/{criado.Id}", new ModuloUpdateRequest("Módulo Atualizado", "50 min", "Nova Desc"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Módulo Atualizado", atualizado.Nome);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var trilha = await CriarTrilhaAuxiliarAsync("Trilha Para Modulo Del");
        var postRes = await _client.PostAsJsonAsync("/api/modulo", new ModuloRequest("Módulo Para Deletar", "10 min", "Desc", trilha.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/modulo/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/modulo/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new ModuloRequest("", "1h", "Desc", Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/modulo", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
