using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class TarefaControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(PetResponse Pet, UsuarioResponse Usuario)> CriarContextoAuxiliarAsync()
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            "Pet Tarefa",
            DateTime.UtcNow.AddYears(-1),
            SexoPet.Femea,
            PortePet.Pequeno,
            false,
            racas.First().Id
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);

        var telPost = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "988887777"));
        var tel = await telPost.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(tel);

        var email = $"cuidador_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var userPost = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Cuidador Tarefa", email, "senha123", RoleUsuario.Comum, tel.Id));
        var user = await userPost.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(user);

        await _client.PostAsJsonAsync("/api/usuariopet", new UsuarioPetRequest(user.Id, pet.Id, ResponPrinc: true));

        return (pet, user);
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var (pet, user) = await CriarContextoAuxiliarAsync();
        var request = new TarefaRequest("Dar Ração Especial", 25, "Ração hipoalergênica 100g", DateTime.UtcNow.AddDays(1), pet.Id, user.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tarefa", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tarefa = await response.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(tarefa);
        Assert.Equal("Dar Ração Especial", tarefa.Titulo);
        Assert.Equal(25, tarefa.PontosTarefa);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var (pet, user) = await CriarContextoAuxiliarAsync();
        var postRes = await _client.PostAsJsonAsync("/api/tarefa", new TarefaRequest("Passeio Diário", 15, "Caminhar 20 min", DateTime.UtcNow.AddDays(1), pet.Id, user.Id));
        var criada = await postRes.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(criada);

        // Act
        var response = await _client.GetAsync($"/api/tarefa/{criada.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lida = await response.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(lida);
        Assert.Equal(criada.Id, lida.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tarefa/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Concluir_TarefaPendenteValida_DeveRetornar200OkEConcluirTarefa()
    {
        // Arrange
        var (pet, user) = await CriarContextoAuxiliarAsync();
        var postRes = await _client.PostAsJsonAsync("/api/tarefa", new TarefaRequest("Administrar Medicamento", 50, "Antibiótico às 14h", DateTime.UtcNow.AddDays(1), pet.Id, user.Id));
        var criada = await postRes.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(criada);

        // Act
        var concluirRes = await _client.PostAsJsonAsync($"/api/tarefa/{criada.Id}/concluir", new TarefaConcluirRequest(user.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, concluirRes.StatusCode);
        var tarefaConcluida = await concluirRes.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(tarefaConcluida);
        Assert.NotNull(tarefaConcluida.Conclusao);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var (pet, user) = await CriarContextoAuxiliarAsync();
        var postRes = await _client.PostAsJsonAsync("/api/tarefa", new TarefaRequest("Tarefa Deletar", 10, "Desc", DateTime.UtcNow.AddDays(1), pet.Id, user.Id));
        var criada = await postRes.Content.ReadFromJsonAsync<TarefaResponse>();
        Assert.NotNull(criada);

        // Act
        var delRes = await _client.DeleteAsync($"/api/tarefa/{criada.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/tarefa/{criada.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComPrazoInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var (pet, user) = await CriarContextoAuxiliarAsync();
        var requestInvalido = new TarefaRequest("Tarefa Vencida", 10, "Prazo no passado", DateTime.UtcNow.AddDays(-2), pet.Id, user.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tarefa", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
