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
public class HistoricoServiceTests(TestFixture fixture)
{
    private readonly Mock<IHistoricoRepository> _historicoRepoMock = new();
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<ILogger<HistoricoService>> _loggerMock = new();

    private HistoricoService CreateService() =>
        new(_historicoRepoMock.Object, _petRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_PetExistente_DeveCriarHistorico()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var request = new HistoricoRequest("VACINACAO", DateTime.UtcNow, petId);

        _petRepoMock.Setup(r => r.ExistsById(petId)).Returns(true);
        _historicoRepoMock.Setup(r => r.Add(It.IsAny<Historico>())).Returns<Historico>(h => h);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("VACINACAO", response.TipoHist);
        _historicoRepoMock.Verify(r => r.Add(It.IsAny<Historico>()), Times.Once);
    }

    [Fact]
    public void Create_PetInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var request = new HistoricoRequest("VACINACAO", DateTime.UtcNow, petId);

        _petRepoMock.Setup(r => r.ExistsById(petId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Pet não encontrado.", ex.Message);
    }

    [Fact]
    public void GetByPetId_PetComHistorico_DeveRetornarLista()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var hist = fixture.CriarHistoricoValido("CONSULTA", petId);

        _historicoRepoMock.Setup(r => r.GetByPetId(petId)).Returns([hist]);

        // Act
        var result = service.GetByPetId(petId);

        // Assert
        Assert.Single(result);
        Assert.Equal("CONSULTA", result[0].TipoHist);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _historicoRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _historicoRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
