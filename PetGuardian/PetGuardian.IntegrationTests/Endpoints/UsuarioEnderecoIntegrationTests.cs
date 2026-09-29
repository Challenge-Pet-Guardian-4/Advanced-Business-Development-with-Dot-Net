using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class UsuarioEnderecoIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(UsuarioResponse Usuario, EnderecoResponse Endereco)> CriarContextoAuxiliarAsync()
    {
        var telPost = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "955554444"));
        Assert.Equal(HttpStatusCode.Created, telPost.StatusCode);
        var tel = await telPost.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(tel);

        var email = $"u_{Guid.NewGuid():N}"[..25] + "@email.com";
        var userPost = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Maria Endereco", email, "Senha@123", RoleUsuario.Comum, tel.Id));
        var user = await userPost.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(user);

        var endPost = await _client.PostAsJsonAsync("/api/endereco", new EnderecoRequest("01001-000", "100"));
        var end = await endPost.Content.ReadFromJsonAsync<EnderecoResponse>();
        Assert.NotNull(end);

        return (user, end);
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var (user, end) = await CriarContextoAuxiliarAsync();
        var request = new UsuarioEnderecoRequest(user.Id, end.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuarioendereco", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var vinculo = await response.Content.ReadFromJsonAsync<UsuarioEnderecoResponse>();
        Assert.NotNull(vinculo);
        Assert.Equal(user.Id, vinculo.UsuarioId);
        Assert.Equal(end.Id, vinculo.EnderecoId);
    }

    [Fact]
    public async Task GetByUsuario_UsuarioExistente_DeveRetornarListaCom200Ok()
    {
        // Arrange
        var (user, end) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuarioendereco", new UsuarioEnderecoRequest(user.Id, end.Id));

        // Act
        var response = await _client.GetAsync($"/api/usuarioendereco/by-usuario/{user.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<UsuarioEnderecoResponse>>();
        Assert.NotNull(lista);
        Assert.Contains(lista, v => v.EnderecoId == end.Id);
    }

    [Fact]
    public async Task GetByEndereco_EnderecoExistente_DeveRetornarListaCom200Ok()
    {
        // Arrange
        var (user, end) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuarioendereco", new UsuarioEnderecoRequest(user.Id, end.Id));

        // Act
        var response = await _client.GetAsync($"/api/usuarioendereco/by-endereco/{end.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<UsuarioEnderecoResponse>>();
        Assert.NotNull(lista);
        Assert.Contains(lista, v => v.UsuarioId == user.Id);
    }

    [Fact]
    public async Task Delete_VinculoExistente_DeveRetornar204NoContent()
    {
        // Arrange
        var (user, end) = await CriarContextoAuxiliarAsync();
        await _client.PostAsJsonAsync("/api/usuarioendereco", new UsuarioEnderecoRequest(user.Id, end.Id));

        // Act
        var delResponse = await _client.DeleteAsync($"/api/usuarioendereco/{user.Id}/{end.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delResponse.StatusCode);
    }

    [Fact]
    public async Task Post_ComIdsInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new UsuarioEnderecoRequest(Guid.Empty, Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuarioendereco", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
