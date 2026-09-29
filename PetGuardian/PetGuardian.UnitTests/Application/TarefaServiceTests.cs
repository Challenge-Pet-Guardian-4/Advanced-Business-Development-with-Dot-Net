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
public class TarefaServiceTests(TestFixture fixture)
{
    private readonly Mock<ITarefaRepository> _tarefaRepoMock = new();
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<IRepository<Status>> _statusRepoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IUsuarioPetRepository> _usuarioPetRepoMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepoMock = new();
    private readonly Mock<ILogger<TarefaService>> _loggerMock = new();

    private TarefaService CreateService() =>
        new(_tarefaRepoMock.Object,
            _petRepoMock.Object,
            _statusRepoMock.Object,
            _usuarioRepoMock.Object,
            _usuarioPetRepoMock.Object,
            _historicoRepoMock.Object,
            _loggerMock.Object);

    [Fact]
    public void Create_DadosValidosECuidadorVinculado_DeveCriarTarefaComStatusPendente()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var statusPendente = new Status("PENDENTE");

        var request = new TarefaRequest("Vacinar", 50, "Vacina antirrábica", DateTime.UtcNow.AddDays(5), petId, usuarioId);

        _petRepoMock.Setup(r => r.ExistsById(petId)).Returns(true);
        _usuarioRepoMock.Setup(r => r.ExistsById(usuarioId)).Returns(true);
        _usuarioPetRepoMock.Setup(r => r.Exists(usuarioId, petId)).Returns(true);
        _statusRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Status, bool>>>())).Returns(statusPendente);
        _tarefaRepoMock.Setup(r => r.Add(It.IsAny<Tarefa>())).Returns<Tarefa>(t => t);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Vacinar", response.Titulo);
        _tarefaRepoMock.Verify(r => r.Add(It.IsAny<Tarefa>()), Times.Once);
    }

    [Fact]
    public void Create_CuidadorNaoVinculado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var request = new TarefaRequest("Vacinar", 50, "Vacina", DateTime.UtcNow.AddDays(5), petId, usuarioId);

        _petRepoMock.Setup(r => r.ExistsById(petId)).Returns(true);
        _usuarioRepoMock.Setup(r => r.ExistsById(usuarioId)).Returns(true);
        _usuarioPetRepoMock.Setup(r => r.Exists(usuarioId, petId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Somente cuidadores vinculados ao pet podem receber tarefas.", ex.Message);
        _tarefaRepoMock.Verify(r => r.Add(It.IsAny<Tarefa>()), Times.Never);
    }

    [Fact]
    public void Concluir_CuidadorAutorizado_DeveConcluirEGravarHistorico()
    {
        // Arrange
        var service = CreateService();
        var petId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var statusConcluido = new Status("CONCLUIDO");
        var tarefa = fixture.CriarTarefaValida(petId: petId, usuarioId: usuarioId);

        _tarefaRepoMock.Setup(r => r.GetById(tarefa.Id)).Returns(tarefa);
        _usuarioPetRepoMock.Setup(r => r.Exists(usuarioId, petId)).Returns(true);
        _statusRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Status, bool>>>())).Returns(statusConcluido);
        _tarefaRepoMock.Setup(r => r.Update(tarefa)).Returns(tarefa);

        // Act
        var response = service.Concluir(tarefa.Id, usuarioId);

        // Assert
        Assert.NotNull(response.Conclusao);
        _tarefaRepoMock.Verify(r => r.Update(tarefa), Times.Once);
        _historicoRepoMock.Verify(r => r.Add(It.Is<Historico>(h => h.TipoHist == "TAREFA_CONCLUIDA" && h.PetId == petId)), Times.Once);
    }
}
