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
public class RacaServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Raca>> _racaRepoMock = new();
    private readonly Mock<ILogger<RacaService>> _loggerMock = new();

    private RacaService CreateService() =>
        new(_racaRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_NomeValido_DeveCriarRaca()
    {
        // Arrange
        var service = CreateService();
        var request = new RacaRequest("Labrador");

        _racaRepoMock.Setup(r => r.Add(It.IsAny<Raca>())).Returns<Raca>(r => r);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Labrador", response.NomeRaca);
        _racaRepoMock.Verify(r => r.Add(It.IsAny<Raca>()), Times.Once);
    }

    [Fact]
    public void GetById_RacaExistente_DeveRetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var raca = fixture.CriarRacaValida("Poodle");
        _racaRepoMock.Setup(r => r.GetById(raca.Id)).Returns(raca);

        // Act
        var response = service.GetById(raca.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Poodle", response.NomeRaca);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _racaRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _racaRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
