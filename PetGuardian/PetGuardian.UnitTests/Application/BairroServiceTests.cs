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
public class BairroServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Bairro>> _bairroRepoMock = new();
    private readonly Mock<IRepository<Cidade>> _cidadeRepoMock = new();
    private readonly Mock<ILogger<BairroService>> _loggerMock = new();

    private BairroService CreateService() =>
        new(_bairroRepoMock.Object, _cidadeRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_CidadeExistente_DeveCriarBairro()
    {
        // Arrange
        var service = CreateService();
        var cidadeId = Guid.NewGuid();
        var request = new BairroRequest("Vila Madalena", cidadeId);

        _cidadeRepoMock.Setup(r => r.ExistsById(cidadeId)).Returns(true);
        _bairroRepoMock.Setup(r => r.Add(It.IsAny<Bairro>())).Returns<Bairro>(b => b);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Vila Madalena", response.NomeBairro);
        _bairroRepoMock.Verify(r => r.Add(It.IsAny<Bairro>()), Times.Once);
    }

    [Fact]
    public void Create_CidadeInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var cidadeId = Guid.NewGuid();
        var request = new BairroRequest("Vila Madalena", cidadeId);

        _cidadeRepoMock.Setup(r => r.ExistsById(cidadeId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Cidade não encontrada.", ex.Message);
        _bairroRepoMock.Verify(r => r.Add(It.IsAny<Bairro>()), Times.Never);
    }

    [Fact]
    public void GetByCidadeId_CidadeComBairros_DeveRetornarLista()
    {
        // Arrange
        var service = CreateService();
        var cidadeId = Guid.NewGuid();
        var bairro = fixture.CriarBairroValido("Pinheiros", cidadeId);

        _bairroRepoMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Bairro, bool>>>()))
            .Returns([bairro]);

        // Act
        var result = service.GetByCidadeId(cidadeId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Pinheiros", result[0].NomeBairro);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _bairroRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _bairroRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
