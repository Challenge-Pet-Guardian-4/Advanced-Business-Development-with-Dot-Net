using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class AuthServiceTests(TestFixture fixture)
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();

    private AuthService CreateService() => new(_usuarioRepoMock.Object, _tokenServiceMock.Object, _loggerMock.Object);

    [Fact]
    public void Login_CredenciaisValidas_DeveRetornarTokenEPerfil()
    {
        // Arrange
        var service = CreateService();
        var usuario = fixture.CriarUsuarioValido(email: "carlos@email.com");
        _usuarioRepoMock.Setup(r => r.GetByEmail("carlos@email.com")).Returns(usuario);
        _tokenServiceMock.Setup(t => t.GerarToken(usuario)).Returns("jwt-fake");

        // Act
        var resposta = service.Login(new LoginRequest("  Carlos@Email.com ", "senhaSegura123"));

        // Assert
        Assert.NotNull(resposta);
        Assert.Equal("jwt-fake", resposta.Token);
        Assert.Equal("Bearer", resposta.Tipo);
        Assert.Equal(usuario.Id, resposta.User.Id);
    }

    [Fact]
    public void Login_SenhaIncorreta_DeveRetornarNullENaoGerarToken()
    {
        // Arrange
        var service = CreateService();
        var usuario = fixture.CriarUsuarioValido(email: "carlos@email.com");
        _usuarioRepoMock.Setup(r => r.GetByEmail("carlos@email.com")).Returns(usuario);

        // Act
        var resposta = service.Login(new LoginRequest("carlos@email.com", "senhaErrada123"));

        // Assert
        Assert.Null(resposta);
        _tokenServiceMock.Verify(t => t.GerarToken(It.IsAny<PetGuardian.Domain.Entities.Usuario>()), Times.Never);
    }

    [Fact]
    public void Login_EmailInexistente_DeveRetornarNull()
    {
        // Arrange
        var service = CreateService();
        _usuarioRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((PetGuardian.Domain.Entities.Usuario?)null);

        // Act
        var resposta = service.Login(new LoginRequest("naoexiste@email.com", "qualquer123"));

        // Assert
        Assert.Null(resposta);
    }

    [Fact]
    public void GetPerfil_UsuarioInexistente_DeveRetornarNull()
    {
        // Arrange
        var service = CreateService();
        _usuarioRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((PetGuardian.Domain.Entities.Usuario?)null);

        // Act
        var perfil = service.GetPerfil(Guid.NewGuid());

        // Assert
        Assert.Null(perfil);
    }
}