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
public class CidadeServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Cidade>> _cidadeRepoMock = new();
    private readonly Mock<IRepository<Estado>> _estadoRepoMock = new();
    private readonly Mock<ILogger<CidadeService>> _loggerMock = new();

    private CidadeService CreateService() =>
        new(_cidadeRepoMock.Object, _estadoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_EstadoExistente_DeveCriarCidade()
    {
        // Arrange
        var service = CreateService();
        var estadoId = Guid.NewGuid();
        var request = new CidadeRequest("Campinas", estadoId);

        _estadoRepoMock.Setup(r => r.ExistsById(estadoId)).Returns(true);
        _cidadeRepoMock.Setup(r => r.Add(It.IsAny<Cidade>())).Returns<Cidade>(c => c);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Campinas", response.NomeCidade);
        _cidadeRepoMock.Verify(r => r.Add(It.IsAny<Cidade>()), Times.Once);
    }

    [Fact]
    public void Create_EstadoInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var estadoId = Guid.NewGuid();
        var request = new CidadeRequest("Campinas", estadoId);

        _estadoRepoMock.Setup(r => r.ExistsById(estadoId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Estado não encontrado.", ex.Message);
    }

    [Fact]
    public void GetByEstadoId_EstadoComCidades_DeveRetornarLista()
    {
        // Arrange
        var service = CreateService();
        var estadoId = Guid.NewGuid();
        var cidade = fixture.CriarCidadeValida("Campinas", estadoId);

        _cidadeRepoMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Cidade, bool>>>()))
            .Returns([cidade]);

        // Act
        var result = service.GetByEstadoId(estadoId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Campinas", result[0].NomeCidade);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _cidadeRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _cidadeRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
