using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.Common;
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
    private readonly Mock<IConteudoAulaRepository> _conteudoRepoMock = new();
    private readonly Mock<ILogger<AulaService>> _loggerMock = new();

    private AulaService CreateService() =>
        new(_aulaRepoMock.Object, _moduloRepoMock.Object, _conteudoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Search_ComFiltro_DeveRetornarPaginaEnriquecidaComConteudo()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var aula = fixture.CriarAulaValida(moduloId);
        var conteudo = new ConteudoAulaResponse("m1", aula.Id, "ARTIGO", "# Markdown", [], DateTime.UtcNow);

        _aulaRepoMock.Setup(r => r.GetPaged(It.IsAny<Expression<Func<Aula, bool>>?>(), "Nome", false, 1, 10))
            .Returns(new PagedResult<Aula>([aula], 1));
        _conteudoRepoMock.Setup(r => r.GetByAulaIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConteudoAulaResponse> { [aula.Id] = conteudo });

        // Act
        var resultado = service.Search(new PageQuery(), new AulaFilter { ModuloId = moduloId });

        // Assert
        Assert.Single(resultado.Items);
        Assert.Equal("# Markdown", resultado.Items[0].Conteudo);
        Assert.Equal(1, resultado.TotalCount);
    }

    [Fact]
    public void ConcluirEDesmarcar_AulaExistente_DeveAlternarFlagEPersistir()
    {
        // Arrange
        var service = CreateService();
        var aula = fixture.CriarAulaValida();
        _aulaRepoMock.Setup(r => r.GetById(aula.Id)).Returns(aula);
        _aulaRepoMock.Setup(r => r.Update(aula)).Returns(aula);

        // Act
        var concluida = service.Concluir(aula.Id);
        var desmarcada = service.Desmarcar(aula.Id);

        // Assert
        Assert.NotNull(concluida);
        Assert.True(concluida.Concluida);
        Assert.NotNull(desmarcada);
        Assert.False(desmarcada.Concluida);
        _aulaRepoMock.Verify(r => r.Update(aula), Times.Exactly(2));
    }

    [Fact]
    public void Create_ModuloValido_DeveCriarAula()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var request = new AulaRequest("Aula 1: Sentar", "Descricao", 20, "Facil", false, moduloId);

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
        var request = new AulaRequest("Aula 1", "Desc", 10, "Facil", false, moduloId);

        _moduloRepoMock.Setup(m => m.ExistsById(moduloId)).Returns(false);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Módulo não encontrado.", ex.Message);
    }

    [Fact]
    public void GetByModuloId_ModuloComAulas_DeveRetornarListaEnriquecidaComConteudo()
    {
        // Arrange
        var service = CreateService();
        var moduloId = Guid.NewGuid();
        var aula = fixture.CriarAulaValida(moduloId);
        var conteudo = new ConteudoAulaResponse("m1", aula.Id, "ARTIGO", "# Guia", [], DateTime.UtcNow);

        _aulaRepoMock.Setup(r => r.GetByModuloId(moduloId)).Returns([aula]);
        _conteudoRepoMock.Setup(r => r.GetByAulaIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConteudoAulaResponse> { [aula.Id] = conteudo });

        // Act
        var result = service.GetByModuloId(moduloId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Aula 1: Sentar", result[0].Nome);
        Assert.Equal("# Guia", result[0].Conteudo);
    }

    [Fact]
    public async Task DeleteAsync_IdValido_DeveRetornarTrueERemoverConteudoNoMongo()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = await service.DeleteAsync(id);

        // Assert
        Assert.True(resultado);
        _aulaRepoMock.Verify(r => r.Delete(id), Times.Once);
        _conteudoRepoMock.Verify(r => r.ExcluirPorAulaIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AulaInexistente_DeveRetornarFalseSemTocarNoMongo()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.Delete(id)).Returns(false);

        // Act
        var resultado = await service.DeleteAsync(id);

        // Assert
        Assert.False(resultado);
        _conteudoRepoMock.Verify(r => r.ExcluirPorAulaIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_MongoIndisponivel_DeveManterExclusaoRelacional()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.Delete(id)).Returns(true);
        _conteudoRepoMock.Setup(r => r.ExcluirPorAulaIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo fora"));

        // Act
        var resultado = await service.DeleteAsync(id);

        // Assert
        Assert.True(resultado);
    }
}
