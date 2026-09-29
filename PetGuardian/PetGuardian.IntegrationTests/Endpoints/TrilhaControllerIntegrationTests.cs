using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class TrilhaControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<PetResponse> CriarPetAuxiliarAsync(string nome)
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);
        var racaId = racas.First().Id;

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            nome,
            DateTime.UtcNow.AddYears(-1),
            SexoPet.Femea,
            PortePet.Pequeno,
            false,
            racaId
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);
        return pet;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Trilha Post");
        var request = new TrilhaRequest("Trilha de Adestramento Básico", "Desc Trilha", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/trilha", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var trilha = await response.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(trilha);
        Assert.Equal("Trilha de Adestramento Básico", trilha.Nome);
        Assert.Equal(pet.Id, trilha.PetId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Trilha Get");
        var postRes = await _client.PostAsJsonAsync("/api/trilha", new TrilhaRequest("Trilha Nutrição", "Desc", pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/trilha/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/trilha/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Trilha Put");
        var postRes = await _client.PostAsJsonAsync("/api/trilha", new TrilhaRequest("Trilha Comportamento", "Original", pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/trilha/{criado.Id}", new TrilhaUpdateRequest("Trilha Comportamento Avançado", "Nova Desc"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Trilha Comportamento Avançado", atualizado.Nome);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Trilha Del");
        var postRes = await _client.PostAsJsonAsync("/api/trilha", new TrilhaRequest("Trilha Para Deletar", "Desc", pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/trilha/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/trilha/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new TrilhaRequest("", "Desc", Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/trilha", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
