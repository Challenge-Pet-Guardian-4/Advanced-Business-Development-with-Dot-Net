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
public class TrilhaCatalogoServiceTests(TestFixture fixture)
{
    private readonly Mock<ITrilhaRepository> _trilhaRepoMock = new();
    private readonly Mock<IModuloRepository> _moduloRepoMock = new();
    private readonly Mock<IAulaRepository> _aulaRepoMock = new();
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<ITrilhaCatalogoRepository> _catalogoRepoMock = new();
    private readonly Mock<ILogger<TrilhaCatalogoService>> _loggerMock = new();

    private TrilhaCatalogoService CreateService() =>
        new(_trilhaRepoMock.Object, _moduloRepoMock.Object, _aulaRepoMock.Object,
            _petRepoMock.Object, _catalogoRepoMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Sincronizar_TrilhaComModuloEAula_DeveGravarDocumentoComEmbutidos()
    {
        // Arrange
        var service = CreateService();
        var pet = fixture.CriarPetValido("Thor");
        var trilha = fixture.CriarTrilhaValida(pet.Id);
        var modulo = fixture.CriarModuloValido(trilha.Id);
        var aula = fixture.CriarAulaValida(modulo.Id);

        _trilhaRepoMock.Setup(r => r.GetAll()).Returns([trilha]);
        _moduloRepoMock.Setup(r => r.GetAll()).Returns([modulo]);
        _aulaRepoMock.Setup(r => r.GetAll()).Returns([aula]);
        _petRepoMock.Setup(r => r.GetAll()).Returns([pet]);

        List<TrilhaCatalogoResponse>? gravados = null;
        _catalogoRepoMock.Setup(r => r.UpsertAsync(It.IsAny<IReadOnlyCollection<TrilhaCatalogoResponse>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<TrilhaCatalogoResponse>, CancellationToken>((docs, _) => gravados = docs.ToList())
            .ReturnsAsync((1, 0));
        _catalogoRepoMock.Setup(r => r.RemoverAusentesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        // Act
        var resumo = await service.SincronizarAsync();

        // Assert
        Assert.Equal(1, resumo.TrilhasRelacionais);
        Assert.Equal(1, resumo.Inseridas);
        var doc = Assert.Single(gravados!);
        Assert.Equal(trilha.Id, doc.TrilhaIdOrigem);
        Assert.Equal("Thor", doc.PetAlvo.Nome);
        Assert.Equal("MEDIO", doc.PetAlvo.Porte);
        var m = Assert.Single(doc.Modulos);
        Assert.Equal(modulo.Nome, m.Titulo);
        Assert.Equal(aula.Nome, Assert.Single(m.Aulas).Titulo);
    }

    [Fact]
    public async Task Sincronizar_TrilhaDeBancoVazioDeAusentes_DeveInformarIdsPresentesParaRemocao()
    {
        // Arrange
        var service = CreateService();
        var pet = fixture.CriarPetValido();
        var trilha = fixture.CriarTrilhaValida(pet.Id);

        _trilhaRepoMock.Setup(r => r.GetAll()).Returns([trilha]);
        _moduloRepoMock.Setup(r => r.GetAll()).Returns([]);
        _aulaRepoMock.Setup(r => r.GetAll()).Returns([]);
        _petRepoMock.Setup(r => r.GetAll()).Returns([pet]);
        _catalogoRepoMock.Setup(r => r.UpsertAsync(It.IsAny<IReadOnlyCollection<TrilhaCatalogoResponse>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, 0));
        _catalogoRepoMock.Setup(r => r.RemoverAusentesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2L);

        // Act
        var resumo = await service.SincronizarAsync();

        // Assert
        Assert.Equal(2, resumo.Removidas);
        _catalogoRepoMock.Verify(r => r.RemoverAusentesAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(trilha.Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sincronizar_TrilhaComPetInexistente_DeveIgnorarTrilha()
    {
        // Arrange
        var service = CreateService();
        var trilhaOrfa = fixture.CriarTrilhaValida(Guid.NewGuid());

        _trilhaRepoMock.Setup(r => r.GetAll()).Returns([trilhaOrfa]);
        _moduloRepoMock.Setup(r => r.GetAll()).Returns([]);
        _aulaRepoMock.Setup(r => r.GetAll()).Returns([]);
        _petRepoMock.Setup(r => r.GetAll()).Returns([]);

        List<TrilhaCatalogoResponse>? gravados = null;
        _catalogoRepoMock.Setup(r => r.UpsertAsync(It.IsAny<IReadOnlyCollection<TrilhaCatalogoResponse>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<TrilhaCatalogoResponse>, CancellationToken>((docs, _) => gravados = docs.ToList())
            .ReturnsAsync((0, 0));
        _catalogoRepoMock.Setup(r => r.RemoverAusentesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        // Act
        await service.SincronizarAsync();

        // Assert
        Assert.Empty(gravados!);
    }
}