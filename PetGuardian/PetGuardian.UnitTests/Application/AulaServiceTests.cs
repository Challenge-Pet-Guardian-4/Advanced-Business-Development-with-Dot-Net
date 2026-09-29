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
public class AulaServiceTests(TestFixture fixture)
{
    private readonly Mock<IAulaRepository> _aulaRepoMock = new();
    private readonly Mock<IModuloRepository> _moduloRepoMock = new();
    private readonly Mock<ILogger<AulaService>> _loggerMock = new();

    private AulaService CreateService() =>
        new(_aulaRepoMock.Object, _moduloRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_ModuloValido_DeveCriarAula()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var request = new AulaRequest("Aula 1: Sentar", "Descricao", 20, "Facil", "Conteudo", false, moduloId);

        _moduloRepoMock.Setup(m => m.ExistsById(moduloId)).Returns(true);
        _aulaRepoMock.Setup(a => a.Add(It.IsAny<Aula>())).Returns<Aula>(a => a);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Aula 1: Sentar", response.Nome);
        _aulaRepoMock.Verify(a => a.Add(It.IsAny<Aula>()), Times.Once);
    }

    [Fact]
    public void Create_ModuloInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var request = new AulaRequest("Aula 1", "Desc", 10, "Facil", "Texto", false, moduloId);

        _moduloRepoMock.Setup(m => m.ExistsById(moduloId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Módulo não encontrado.", ex.Message);
    }

    [Fact]
    public void GetByModuloId_ModuloComAulas_DeveRetornarLista()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var aula = fixture.CriarAulaValida(moduloId);

        _aulaRepoMock.Setup(r => r.GetByModuloId(moduloId)).Returns([aula]);

        // Act
        var result = service.GetByModuloId(moduloId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Aula 1: Sentar", result[0].Nome);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _aulaRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
