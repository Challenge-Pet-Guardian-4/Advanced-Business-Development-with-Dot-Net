using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class UsuarioPetControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(PetResponse Pet, UsuarioResponse Tutor1, UsuarioResponse Tutor2)> CriarContextoAuxiliarAsync()
    {
        var racas = await _client.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        Assert.NotNull(racas);

        var petPost = await _client.PostAsJsonAsync("/api/pet", new PetRequest(
            "Max",
            DateTime.UtcNow.AddYears(-2),
            SexoPet.Macho,
            PortePet.Grande,
            false,
            racas.First().Id
        ));
        var pet = await petPost.Content.ReadFromJsonAsync<PetResponse>();
        Assert.NotNull(pet);

        var tel1 = await (await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "911112222"))).Content.ReadFromJsonAsync<TelefoneResponse>();
        var user1Email = $"tutor1_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var user1Post = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Tutor Principal", user1Email, "senha123", RoleUsuario.Comum, tel1!.Id));
        var user1 = await user1Post.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(user1);

        var tel2 = await (await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "933334444"))).Content.ReadFromJsonAsync<TelefoneResponse>();
        var user2Email = $"tutor2_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var user2Post = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Co Cuidador", user2Email, "senha123", RoleUsuario.Comum, tel2!.Id));
        var user2 = await user2Post.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(user2);

        return (pet, user1, user2);
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var (pet, user1, _) = await CriarContextoAuxiliarAsync();
        var request = new UsuarioPetRequest(user1.Id, pet.Id, ResponPrinc: true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuariopet", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var vinculo = await response.Content.ReadFromJsonAsync<UsuarioPetResponse>();
        Assert.NotNull(vinculo);
        Assert.Equal(user1.Id, vinculo.UsuarioId);
        Assert.Equal(pet.Id, vinculo.PetId);
        Assert.True(vinculo.ResponPrinc);
    }

    [Fact]
    public async Task GetRedeCuidado_UsuarioExistente_DeveRetornar200OkComArvoreCompleta()
    {
        // Arrange
        var (pet, user1, user2) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuariopet", new UsuarioPetRequest(user1.Id, pet.Id, ResponPrinc: true));
        await _client.PostAsJsonAsync("/api/usuariopet/invite/by-usuario", new UsuarioPetInviteByUsuarioRequest(user1.Id, user2.Id, pet.Id));

        // Act
        var response = await _client.GetAsync($"/api/usuariopet/rede-cuidado/{user1.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rede = await response.Content.ReadFromJsonAsync<RedeCuidadoResponse>();
        Assert.NotNull(rede);
        Assert.NotEmpty(rede.Pets);
        Assert.NotEmpty(rede.CoCuidadores);
    }

    [Fact]
    public async Task Put_AlterarResponsabilidadePrincipal_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var (pet, user1, user2) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuariopet", new UsuarioPetRequest(user1.Id, pet.Id, ResponPrinc: true));
        await _client.PostAsJsonAsync("/api/usuariopet/invite/by-usuario", new UsuarioPetInviteByUsuarioRequest(user1.Id, user2.Id, pet.Id));

        // Act
        var putResponse = await _client.PutAsJsonAsync($"/api/usuariopet/{user2.Id}/{pet.Id}", new UsuarioPetUpdateRequest(ResponPrinc: false));

        // Assert
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_DesvincularCoCuidador_DeveRetornar204NoContent()
    {
        // Arrange
        var (pet, user1, user2) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuariopet", new UsuarioPetRequest(user1.Id, pet.Id, ResponPrinc: true));
        await _client.PostAsJsonAsync("/api/usuariopet/invite/by-usuario", new UsuarioPetInviteByUsuarioRequest(user1.Id, user2.Id, pet.Id));

        // Act
        var delResponse = await _client.DeleteAsync($"/api/usuariopet/{user2.Id}/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delResponse.StatusCode);
    }

    [Fact]
    public async Task Post_ComIdsInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new UsuarioPetRequest(Guid.Empty, Guid.Empty, false);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuariopet", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
