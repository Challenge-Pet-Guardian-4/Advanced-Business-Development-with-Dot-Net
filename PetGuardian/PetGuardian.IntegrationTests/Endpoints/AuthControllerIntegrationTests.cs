using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string Email, string Senha, Guid UsuarioId)> CriarUsuarioTesteAsync()
    {
        var email = $"auth_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var senha = "senhaSegura123";
        var userPost = await _client.PostAsJsonAsync("/api/usuario",
            new UsuarioRequest("Usuario Auth Teste", email, senha, RoleUsuario.Comum, "11", "987654321", new EnderecoRequest("01001-000", "100")));
        Assert.Equal(HttpStatusCode.Created, userPost.StatusCode);
        var usuario = await userPost.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(usuario);

        return (email, senha, usuario.Id);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornar200ETokenJwt()
    {
        // Arrange
        var (email, senha, _) = await CriarUsuarioTesteAsync();
        var loginRequest = new LoginRequest(email, senha);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));
        Assert.Equal("Bearer", loginResponse.Tipo);
        Assert.Equal(email, loginResponse.User.Email);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_DeveRetornar401Unauthorized()
    {
        // Arrange
        var (email, _, _) = await CriarUsuarioTesteAsync();
        var loginRequest = new LoginRequest(email, "senhaErrada123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_DeveRetornar401Unauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("inexistente@teste.com", "qualquerSenha123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("formato-invalido-de-email", "senha123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_SemTokenAutenticacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenValido_DeveRetornar200EPerfilComLinks()
    {
        // Arrange
        var (email, senha, id) = await CriarUsuarioTesteAsync();
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, senha));
        var loginData = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(perfil);
        Assert.Equal(id, perfil.Id);
        Assert.Equal(email, perfil.Email);
        Assert.NotNull(perfil.Links);
        Assert.Contains(perfil.Links, l => l.Rel == "self");
        Assert.Contains(perfil.Links, l => l.Rel == "update");
        Assert.Contains(perfil.Links, l => l.Rel == "delete");
    }

    [Fact]
    public async Task Me_ComTokenInvalido_DeveRetornar401Unauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido-aleatorio");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
