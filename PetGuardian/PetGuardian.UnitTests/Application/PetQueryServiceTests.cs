using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class PetQueryServiceTests(TestFixture fixture)
{
    private readonly Mock<IPetRepository> _petRepoMock = new();
    private readonly Mock<ILogger<PetQueryService>> _loggerMock = new();

    private PetQueryService CreateService() => new(_petRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Search_SemFiltros_DeveUsarOrdenacaoPadraoPorNomeEMontarEnvelope()
    {
        // Arrange
        var service = CreateService();
        var pet = fixture.CriarPetValido("Thor");
        _petRepoMock.Setup(r => r.GetPaged(It.IsAny<Expression<Func<Pet, bool>>?>(), "Nome", false, 1, 10))
            .Returns(new PagedResult<Pet>([pet], 25));

        // Act
        var resultado = service.Search(new PageQuery(), new PetFilter());

        // Assert
        Assert.Single(resultado.Items);
        Assert.Equal(25, resultado.TotalCount);
        Assert.Equal(3, resultado.TotalPages);
        _petRepoMock.Verify(r => r.GetPaged(It.IsAny<Expression<Func<Pet, bool>>?>(), "Nome", false, 1, 10), Times.Once);
    }

    [Fact]
    public void Search_ComFiltros_DeveGerarPredicadoQueFiltraPorNomeEPorte()
    {
        // Arrange
        var service = CreateService();
        Expression<Func<Pet, bool>>? capturado = null;
        _petRepoMock.Setup(r => r.GetPaged(It.IsAny<Expression<Func<Pet, bool>>?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<Expression<Func<Pet, bool>>?, string?, bool, int, int>((f, _, _, _, _) => capturado = f)
            .Returns(new PagedResult<Pet>([], 0));

        var thorMedio = fixture.CriarPetValido("Thor");                         // porte Medio
        var rexMedio = fixture.CriarPetValido("Rex");
        var thorGrande = new Pet("Thor", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Grande, false, Guid.NewGuid());

        // Act
        service.Search(new PageQuery(), new PetFilter { Nome = " THO ", Porte = PortePet.Medio });

        // Assert
        Assert.NotNull(capturado);
        var predicado = capturado.Compile();
        Assert.True(predicado(thorMedio));
        Assert.False(predicado(rexMedio));
        Assert.False(predicado(thorGrande));
    }

    [Fact]
    public void Search_OrdenacaoInformada_DeveRepassarCampoEDirecaoAoRepositorio()
    {
        // Arrange
        var service = CreateService();
        _petRepoMock.Setup(r => r.GetPaged(It.IsAny<Expression<Func<Pet, bool>>?>(), "DataNascimento", true, 2, 5))
            .Returns(new PagedResult<Pet>([], 0));

        // Act
        service.Search(new PageQuery { Page = 2, PageSize = 5, SortBy = "DataNascimento", SortDir = "DESC" }, new PetFilter());

        // Assert
        _petRepoMock.Verify(r => r.GetPaged(It.IsAny<Expression<Func<Pet, bool>>?>(), "DataNascimento", true, 2, 5), Times.Once);
    }
}