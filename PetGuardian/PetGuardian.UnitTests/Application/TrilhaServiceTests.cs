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
public class TrilhaServiceTests(TestFixture fixture)
{
    private readonly Mock<ITrilhaRepository> _trilhaRepoMock = new();
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<ILogger<TrilhaService>> _loggerMock = new();

    private TrilhaService CreateService() =>
        new(_trilhaRepoMock.Object, _petRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_PetValido_DeveCriarTrilha()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var request = new TrilhaRequest("Trilha Filhote", "Cuidados iniciais", petId);

        _petRepoMock.Setup(p => p.ExistsById(petId)).Returns(true);
        _trilhaRepoMock.Setup(t => t.Add(It.IsAny<Trilha>())).Returns<Trilha>(t => t);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Trilha Filhote", response.Nome);
        _trilhaRepoMock.Verify(t => t.Add(It.IsAny<Trilha>()), Times.Once);
    }

    [Fact]
    public void Create_PetInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var request = new TrilhaRequest("Trilha Filhote", "Cuidados", petId);

        _petRepoMock.Setup(p => p.ExistsById(petId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Pet não encontrado.", ex.Message);
    }

    [Fact]
    public void GetByPetId_PetComTrilhas_DeveRetornarLista()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var trilha = fixture.CriarTrilhaValida(petId);

        _trilhaRepoMock.Setup(r => r.GetByPetId(petId)).Returns([trilha]);

        // Act
        var result = service.GetByPetId(petId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Trilha de Adestramento", result[0].Nome);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _trilhaRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _trilhaRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
