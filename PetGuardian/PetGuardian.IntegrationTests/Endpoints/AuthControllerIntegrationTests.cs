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
        var telPost = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "987654321"));
        Assert.Equal(HttpStatusCode.Created, telPost.StatusCode);
        var tel = await telPost.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(tel);

        var email = $"auth_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var senha = "senhaSegura123";
        var userPost = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Usuario Auth Teste", email, senha, RoleUsuario.Comum, tel.Id));
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
    public async Task LoginMobile_RotaAlternativa_DeveRetornar200EToken()
    {
        // Arrange
        var (email, senha, _) = await CriarUsuarioTesteAsync();
        var loginRequest = new LoginRequest(email, senha);

        // Act
        var response = await _client.PostAsJsonAsync("/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));
        Assert.Equal(email, loginResponse.User.Email);
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
    public async Task Me_ComTokenJwtValido_DeveRetornar200EUsuarioAutenticado()
    {
        // Arrange
        var (email, senha, _) = await CriarUsuarioTesteAsync();
        var loginPost = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, senha));
        var loginResult = await loginPost.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var usuarioMe = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(usuarioMe);
        Assert.Equal(email, usuarioMe.Email);
    }

    [Fact]
    public async Task Me_ComTokenInvalido_DeveRetornar401Unauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token_completamente_falso_invalido");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
