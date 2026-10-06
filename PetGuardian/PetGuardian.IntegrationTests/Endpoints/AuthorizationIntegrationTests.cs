using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class AuthorizationIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _anonimo = factory.CreateClient();

    [Theory]
    [InlineData("/api/pet")]
    [InlineData("/api/tarefa")]
    [InlineData("/api/usuario")]
    [InlineData("/api/estado")]
    [InlineData("/api/catalogo/trilhas")]
    public async Task Get_SemToken_DeveRetornar401Unauthorized(string rota)
    {
        // Act
        var response = await _anonimo.GetAsync(rota);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthLive_SemToken_DeveRetornar200Ok()
    {
        // Act
        var response = await _anonimo.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Estado_UsuarioComum_DeveRetornar403Forbidden()
    {
        // Arrange
        var comum = factory.CreateAuthenticatedClient(RoleUsuario.Comum);

        // Act
        var response = await comum.PostAsJsonAsync("/api/estado", new EstadoRequest("Estado Proibido"));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_Estado_UsuarioComum_DeveRetornar200Ok()
    {
        // Arrange
        var comum = factory.CreateAuthenticatedClient(RoleUsuario.Comum);

        // Act
        var response = await comum.GetAsync("/api/estado");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Estado_Admin_DeveRetornar201Created()
    {
        // Arrange
        var admin = factory.CreateAuthenticatedClient(RoleUsuario.Admin);

        // Act
        var response = await admin.PostAsJsonAsync("/api/estado", new EstadoRequest("Estado Permitido"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_Usuarios_UsuarioComum_DeveRetornar403Forbidden()
    {
        // Arrange
        var comum = factory.CreateAuthenticatedClient(RoleUsuario.Comum);

        // Act
        var response = await comum.GetAsync("/api/usuario");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_Telefone_Anonimo_DeveRetornar201Created()
    {
        // Act
        var response = await _anonimo.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "977776666"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_UsuarioAdmin_Anonimo_DeveRetornar403Forbidden()
    {
        // Arrange
        var request = new UsuarioRequest("Invasor", "invasor@teste.com", "senha123", RoleUsuario.Admin, Guid.NewGuid());

        // Act
        var response = await _anonimo.PostAsJsonAsync("/api/usuario", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_Usuario_ComumTentandoSePromoverAAdmin_DeveRetornar403Forbidden()
    {
        // Arrange
        var tel = await (await _anonimo.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "966665555")))
            .Content.ReadFromJsonAsync<TelefoneResponse>();
        var email = $"u{Guid.NewGuid():N}"[..20] + "@teste.com";
        var usuario = await (await _anonimo.PostAsJsonAsync("/api/usuario",
                new UsuarioRequest("Usuario Comum", email, "senhaSegura123", RoleUsuario.Comum, tel!.Id)))
            .Content.ReadFromJsonAsync<UsuarioResponse>();

        var login = await (await _anonimo.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "senhaSegura123")))
            .Content.ReadFromJsonAsync<LoginResponse>();

        var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        // Act
        var response = await cliente.PutAsJsonAsync($"/api/usuario/{usuario!.Id}",
            new UsuarioUpdateRequest("Usuario Comum", email, "senhaSegura123", RoleUsuario.Admin));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}