using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class AulaControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<ModuloResponse> CriarModuloAuxiliarAsync(string nome)
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            "Pet Aula",
            DateTime.UtcNow.AddYears(-1),
            SexoPet.Femea,
            PortePet.Pequeno,
            false,
            racas.First().Id
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);

        var trilhaPost = await _client.PostAsJsonAsync("/api/trilha", new TrilhaRequest("Trilha Aula", "Desc", pet.Id));
        var trilha = await trilhaPost.Content.ReadFromJsonAsync<TrilhaResponse>();
        Assert.NotNull(trilha);

        var moduloPost = await _client.PostAsJsonAsync("/api/modulo", new ModuloRequest(nome, "30 min", "Desc", trilha.Id));
        var modulo = await moduloPost.Content.ReadFromJsonAsync<ModuloResponse>();
        Assert.NotNull(modulo);
        return modulo;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var modulo = await CriarModuloAuxiliarAsync("Módulo Para Aula Post");
        var request = new AulaRequest("Aula 1: Primeiros Passos", "Como interagir", 15, "Iniciante", "Vídeo e texto explicativo", false, modulo.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/aula", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aula = await response.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(aula);
        Assert.Equal("Aula 1: Primeiros Passos", aula.Nome);
        Assert.Equal(modulo.Id, aula.ModuloId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var modulo = await CriarModuloAuxiliarAsync("Módulo Para Aula Get");
        var postRes = await _client.PostAsJsonAsync("/api/aula", new AulaRequest("Aula Prática", "Desc", 20, "Médio", "Texto", false, modulo.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/aula/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/aula/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var modulo = await CriarModuloAuxiliarAsync("Módulo Para Aula Put");
        var postRes = await _client.PostAsJsonAsync("/api/aula", new AulaRequest("Aula Original", "Desc", 10, "Fácil", "Texto", false, modulo.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/aula/{criado.Id}", new AulaUpdateRequest("Aula Concluída", "Nova Desc", 25, "Fácil", "Texto revisado", true));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Aula Concluída", atualizado.Nome);
        Assert.True(atualizado.Concluida);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var modulo = await CriarModuloAuxiliarAsync("Módulo Para Aula Del");
        var postRes = await _client.PostAsJsonAsync("/api/aula", new AulaRequest("Aula Para Deletar", "Desc", 5, "Fácil", "Texto", false, modulo.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/aula/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/aula/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new AulaRequest("", "Desc", 0, "Iniciante", "Texto", false, Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/aula", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
