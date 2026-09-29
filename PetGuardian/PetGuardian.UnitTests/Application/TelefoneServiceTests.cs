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
public class TelefoneServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Telefone>> _telefoneRepoMock = new();
    private readonly Mock<ILogger<TelefoneService>> _loggerMock = new();

    private TelefoneService CreateService() =>
        new(_telefoneRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_DadosValidos_DeveCriarTelefone()
    {
        // Arrange
        var service = CreateService();
        var request = new TelefoneRequest("11", "987654321");

        _telefoneRepoMock.Setup(r => r.Add(It.IsAny<Telefone>())).Returns<Telefone>(t => t);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("11", response.NumDdd);
        Assert.Equal("987654321", response.NumTel);
        _telefoneRepoMock.Verify(r => r.Add(It.IsAny<Telefone>()), Times.Once);
    }

    [Fact]
    public void GetById_TelefoneExistente_DeveRetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var tel = fixture.CriarTelefoneValido("11", "912345678");
        _telefoneRepoMock.Setup(r => r.GetById(tel.Id)).Returns(tel);

        // Act
        var response = service.GetById(tel.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("11", response.NumDdd);
        Assert.Equal("912345678", response.NumTel);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _telefoneRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _telefoneRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
