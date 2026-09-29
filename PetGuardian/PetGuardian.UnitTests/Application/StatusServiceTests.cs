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
public class StatusServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Status>> _statusRepoMock = new();
    private readonly Mock<ILogger<StatusService>> _loggerMock = new();

    private StatusService CreateService() =>
        new(_statusRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_NomeValido_DeveCriarStatus()
    {
        // Arrange
        var service = CreateService();
        var request = new StatusRequest("PENDENTE");

        _statusRepoMock.Setup(r => r.Add(It.IsAny<Status>())).Returns<Status>(s => s);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("PENDENTE", response.NomeStatus);
        _statusRepoMock.Verify(r => r.Add(It.IsAny<Status>()), Times.Once);
    }

    [Fact]
    public void GetById_StatusExistente_DeveRetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var status = fixture.CriarStatusValido("CONCLUIDO");
        _statusRepoMock.Setup(r => r.GetById(status.Id)).Returns(status);

        // Act
        var response = service.GetById(status.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("CONCLUIDO", response.NomeStatus);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _statusRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _statusRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
