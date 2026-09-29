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
public class ModuloServiceTests(TestFixture fixture)
{
    private readonly Mock<IModuloRepository> _moduloRepoMock = new();
    private readonly Mock<ITrilhaRepository> _trilhaRepoMock = new();
    private readonly Mock<ILogger<ModuloService>> _loggerMock = new();

    private ModuloService CreateService() =>
        new(_moduloRepoMock.Object, _trilhaRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_TrilhaValida_DeveCriarModulo()
    {
        // Arrange
        var service = CreateService();
        var trilhaId = Guid.NewGuid();
        var request = new ModuloRequest("Módulo 1", "2 horas", "Descricao", trilhaId);

        _trilhaRepoMock.Setup(t => t.ExistsById(trilhaId)).Returns(true);
        _moduloRepoMock.Setup(m => m.Add(It.IsAny<Modulo>())).Returns<Modulo>(m => m);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Módulo 1", response.Nome);
        _moduloRepoMock.Verify(m => m.Add(It.IsAny<Modulo>()), Times.Once);
    }

    [Fact]
    public void Create_TrilhaInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var trilhaId = Guid.NewGuid();
        var request = new ModuloRequest("Módulo 1", "2 horas", "Descricao", trilhaId);

        _trilhaRepoMock.Setup(t => t.ExistsById(trilhaId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Trilha não encontrada.", ex.Message);
        _moduloRepoMock.Verify(m => m.Add(It.IsAny<Modulo>()), Times.Never);
    }

    [Fact]
    public void Update_ModuloExistente_DeveAtualizarERetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var modulo = fixture.CriarModuloValido();
        var updateRequest = new ModuloUpdateRequest("Modulo Editado", "5 horas", "Nova Desc");

        _moduloRepoMock.Setup(m => m.GetById(modulo.Id)).Returns(modulo);
        _moduloRepoMock.Setup(m => m.Update(modulo)).Returns(modulo);

        // Act
        var response = service.Update(modulo.Id, updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Modulo Editado", response.Nome);
        _moduloRepoMock.Verify(m => m.Update(modulo), Times.Once);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _moduloRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _moduloRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
