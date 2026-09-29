using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class UsuarioControllerIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> ObterTelefoneIdAsync()
    {
        var telPost = await _client.PostAsJsonAsync("/api/telefone", new TelefoneRequest("11", "987654321"));
        Assert.Equal(HttpStatusCode.Created, telPost.StatusCode);
        var tel = await telPost.Content.ReadFromJsonAsync<TelefoneResponse>();
        Assert.NotNull(tel);
        return tel.Id;
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var telId = await ObterTelefoneIdAsync();
        var email = $"user_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var request = new UsuarioRequest("Fernanda Lima", email, "senhaSegura123", RoleUsuario.Comum, telId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuario", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(usuario);
        Assert.Equal("Fernanda Lima", usuario.Nome);
        Assert.Equal(email, usuario.Email);
    }

    [Fact]
    public async Task GetById_IdExistente_DeveRetornar200Ok()
    {
        // Arrange
        var telId = await ObterTelefoneIdAsync();
        var email = $"user_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var postRes = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Carlos Silva", email, "senha123", RoleUsuario.Comum, telId));
        var criado = await postRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(criado);

        // Act
        var response = await _client.GetAsync($"/api/usuario/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lido = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(lido);
        Assert.Equal(criado.Id, lido.Id);
    }

    [Fact]
    public async Task GetById_IdInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/usuario/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var telId = await ObterTelefoneIdAsync();
        var email = $"user_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var postRes = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Usuario Original", email, "senha123", RoleUsuario.Comum, telId));
        var criado = await postRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(criado);

        // Act
        var updateRequest = new UsuarioUpdateRequest("Usuario Atualizado", email, "novaSenhaSegura", RoleUsuario.Premium);
        var putRes = await _client.PutAsJsonAsync($"/api/usuario/{criado.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var atualizado = await putRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal("Usuario Atualizado", atualizado.Nome);
        Assert.Equal(RoleUsuario.Premium, atualizado.Role);
    }

    [Fact]
    public async Task Delete_IdValido_DeveRetornar204NoContent()
    {
        // Arrange
        var telId = await ObterTelefoneIdAsync();
        var email = $"del_{Guid.NewGuid():N}"[..25] + "@teste.com";
        var postRes = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Usuario Deletar", email, "senha123", RoleUsuario.Comum, telId));
        var criado = await postRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(criado);

        // Act
        var delRes = await _client.DeleteAsync($"/api/usuario/{criado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAposDelete = await _client.GetAsync($"/api/usuario/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAposDelete.StatusCode);
    }

    [Fact]
    public async Task Post_ComEmailDuplicado_DeveRetornar400BadRequest()
    {
        // Arrange
        var telId = await ObterTelefoneIdAsync();
        var emailDuplicado = $"dup_{Guid.NewGuid():N}"[..25] + "@teste.com";
        await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Primeiro", emailDuplicado, "senha123", RoleUsuario.Comum, telId));

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuario", new UsuarioRequest("Segundo", emailDuplicado, "senha456", RoleUsuario.Comum, telId));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var requestInvalido = new UsuarioRequest("", "invalido@email.com", "senha", RoleUsuario.Comum, Guid.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/usuario", requestInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
