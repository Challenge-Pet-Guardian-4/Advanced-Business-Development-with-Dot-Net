using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class HistoricoControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<PetResponse> CriarPetAuxiliarAsync(string nome)
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            nome,
            DateTime.UtcNow.AddYears(-2),
            SexoPet.Macho,
            PortePet.Medio,
            true,
            racas.First().Id
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);
        return pet;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Hist Post");
        var request = new HistoricoRequest("VACINACAO_RAIVA", DateTime.UtcNow, pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/historico", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var historico = await response.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(historico);
        Assert.Equal("VACINACAO_RAIVA", historico.TipoHist);
        Assert.Equal(pet.Id, historico.PetId);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Hist Get");
        var postRes = await _client.PostAsJsonAsync("/api/historico", new HistoricoRequest("CHECKUP_ANUAL", DateTime.UtcNow, pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/historico/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/historico/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByPet_PetExistente_DeveRetornarListaCom200Ok()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Hist ByPet");
        var postRes = await _client.PostAsJsonAsync("/api/historico", new HistoricoRequest("PESAGEM_MENSAL", DateTime.UtcNow, pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/historico/by-pet/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<HistoricoResponse>>();
        Assert.NotNull(lista);
        Assert.Contains(lista, h => h.Id == criado.Id);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Hist Put");
        var postRes = await _client.PostAsJsonAsync("/api/historico", new HistoricoRequest("EXAME_SANGUE", DateTime.UtcNow, pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(criado);

        // Act
        var putRes = await _client.PutAsJsonAsync($"/api/historico/{criado.Id}", new HistoricoUpdateRequest("EXAME_SANGUE_CONCLUIDO", DateTime.UtcNow));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("EXAME_SANGUE_CONCLUIDO", atualizado.TipoHist);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var pet = await CriarPetAuxiliarAsync("Pet Hist Del");
        var postRes = await _client.PostAsJsonAsync("/api/historico", new HistoricoRequest("REGISTRO_TEMPORARIO", DateTime.UtcNow, pet.Id));
        var criado = await postRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/historico/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/historico/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new HistoricoRequest("", DateTime.UtcNow, Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/historico", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
