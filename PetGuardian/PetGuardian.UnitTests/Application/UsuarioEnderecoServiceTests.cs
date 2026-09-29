using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Domain.Entities;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class UsuarioEnderecoServiceTests(TestFixture fixture)
{
    private readonly Mock<IUsuarioEnderecoRepository> _usuarioEnderecoRepoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IRepository<Endereco>> _enderecoRepoMock = new();
    private readonly Mock<ILogger<UsuarioEnderecoService>> _loggerMock = new();

    private UsuarioEnderecoService CreateService() =>
        new(_usuarioEnderecoRepoMock.Object, _usuarioRepoMock.Object, _enderecoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_UsuarioEEnderecoExistentes_DeveCriarVinculo()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var endId = Guid.NewGuid();
        var request = new UsuarioEnderecoRequest(userId, endId);
        var vinculo = fixture.CriarUsuarioEnderecoValido(userId, endId);

        _usuarioRepoMock.Setup(r => r.ExistsById(userId)).Returns(true);
        _enderecoRepoMock.Setup(r => r.ExistsById(endId)).Returns(true);
        _usuarioEnderecoRepoMock.Setup(r => r.Add(It.IsAny<UsuarioEndereco>())).Returns(vinculo);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(userId, response.UsuarioId);
        Assert.Equal(endId, response.EnderecoId);
        _usuarioEnderecoRepoMock.Verify(r => r.Add(It.IsAny<UsuarioEndereco>()), Times.Once);
    }

    [Fact]
    public void Create_UsuarioInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var endId = Guid.NewGuid();
        var request = new UsuarioEnderecoRequest(userId, endId);

        _usuarioRepoMock.Setup(r => r.ExistsById(userId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Usuário não encontrado.", ex.Message);
    }

    [Fact]
    public void Create_EnderecoInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var endId = Guid.NewGuid();
        var request = new UsuarioEnderecoRequest(userId, endId);

        _usuarioRepoMock.Setup(r => r.ExistsById(userId)).Returns(true);
        _enderecoRepoMock.Setup(r => r.ExistsById(endId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Endereço não encontrado.", ex.Message);
    }

    [Fact]
    public void Delete_VinculoValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var endId = Guid.NewGuid();
        _usuarioEnderecoRepoMock.Setup(r => r.Delete(userId, endId)).Returns(true);

        // Act
        var resultado = service.Delete(userId, endId);

        // Assert
        Assert.True(resultado);
        _usuarioEnderecoRepoMock.Verify(r => r.Delete(userId, endId), Times.Once);
    }
}
