using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class PetServiceTests(TestFixture fixture)
{
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<IRepository<Raca>> _racaRepoMock = new();
    private readonly Mock<ITarefaRepository> _tarefaRepoMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepoMock = new();
    private readonly Mock<ILogger<PetService>> _loggerMock = new();

    private PetService CreateService() =>
        new(_petRepoMock.Object, _racaRepoMock.Object, _tarefaRepoMock.Object, _historicoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_RacaExistente_DevePersistirECriarPet()
    {
        // Arrange
        var service = CreateService();
        var racaId = Guid.NewGuid();
        var request = new PetRequest("Rex", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, racaId);

        _racaRepoMock.Setup(r => r.ExistsById(racaId)).Returns(true);
        _petRepoMock.Setup(r => r.Add(It.IsAny<Pet>())).Returns<Pet>(p => p);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Rex", response.Nome);
        _petRepoMock.Verify(r => r.Add(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public void Create_RacaInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var racaId = Guid.NewGuid();
        var request = new PetRequest("Rex", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, racaId);

        _racaRepoMock.Setup(r => r.ExistsById(racaId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Raça não encontrada.", ex.Message);
        _petRepoMock.Verify(r => r.Add(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public void Update_PetExistente_DeveAtualizarERetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var pet = fixture.CriarPetValido("Nome Antigo");
        var novaRacaId = Guid.NewGuid();
        var request = new PetRequest("Nome Novo", DateTime.UtcNow.AddYears(-1), SexoPet.Femea, PortePet.Pequeno, true, novaRacaId);

        _petRepoMock.Setup(r => r.GetById(pet.Id)).Returns(pet);
        _racaRepoMock.Setup(r => r.ExistsById(novaRacaId)).Returns(true);
        _petRepoMock.Setup(r => r.Update(It.IsAny<Pet>())).Returns<Pet>(p => p);

        // Act
        var response = service.Update(pet.Id, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Nome Novo", response.Nome);
        Assert.Equal(novaRacaId, response.RacaId);
        _petRepoMock.Verify(r => r.Update(pet), Times.Once);
    }

    [Fact]
    public void Update_PetNaoEncontrado_DeveRetornarNull()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        var request = new PetRequest("Nome", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Medio, false, Guid.NewGuid());

        _petRepoMock.Setup(r => r.GetById(id)).Returns((Pet?)null);

        // Act
        var response = service.Update(id, request);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public void GetHistorico_PetComEventosETarefasConcluidas_DeveRetornarLinhaDoTempoOrdenada()
    {
        // Arrange
        var service = CreateService();
        var pet = fixture.CriarPetValido();
        var dataAntiga = DateTime.UtcNow.AddDays(-5);
        var dataRecente = DateTime.UtcNow.AddDays(-1);

        var historicos = new List<Historico>
        {
            new("VACINA", dataAntiga, pet.Id)
        };

        var tarefa = fixture.CriarTarefaValida(petId: pet.Id);
        tarefa.Concluir();

        _petRepoMock.Setup(r => r.ExistsById(pet.Id)).Returns(true);
        _historicoRepoMock.Setup(r => r.GetByPetId(pet.Id)).Returns(historicos);
        _tarefaRepoMock.Setup(r => r.GetByPetId(pet.Id)).Returns([tarefa]);

        // Act
        var result = service.GetHistorico(pet.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].DataEvento >= result[1].DataEvento); // Ordenado descrescente
    }

    [Fact]
    public void GetByRacaId_RacaComPetsCadastrados_DeveChamarRepositorioERetornarPets()
    {
        // Arrange
        var service = CreateService();
        var racaId = Guid.NewGuid();
        var pet = fixture.CriarPetValido(racaId: racaId);

        _petRepoMock.Setup(r => r.GetByRacaId(racaId)).Returns([pet]);

        // Act
        var result = service.GetByRacaId(racaId);

        // Assert
        Assert.Single(result);
        Assert.Equal(pet.Id, result[0].Id);
        _petRepoMock.Verify(r => r.GetByRacaId(racaId), Times.Once);
    }
}
