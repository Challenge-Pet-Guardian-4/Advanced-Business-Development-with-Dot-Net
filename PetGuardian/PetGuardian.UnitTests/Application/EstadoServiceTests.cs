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
public class EstadoServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Estado>> _estadoRepoMock = new();
    private readonly Mock<ILogger<EstadoService>> _loggerMock = new();

    private EstadoService CreateService() =>
        new(_estadoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_DadosValidos_DeveCriarEstado()
    {
        // Arrange
        var service = CreateService();
        var request = new EstadoRequest("São Paulo");

        _estadoRepoMock.Setup(r => r.Add(It.IsAny<Estado>())).Returns<Estado>(e => e);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("São Paulo", response.NomeEstado);
        _estadoRepoMock.Verify(r => r.Add(It.IsAny<Estado>()), Times.Once);
    }

    [Fact]
    public void GetById_EstadoExistente_DeveRetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var estado = fixture.CriarEstadoValido("Minas Gerais");
        _estadoRepoMock.Setup(r => r.GetById(estado.Id)).Returns(estado);

        // Act
        var response = service.GetById(estado.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Minas Gerais", response.NomeEstado);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _estadoRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _estadoRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
