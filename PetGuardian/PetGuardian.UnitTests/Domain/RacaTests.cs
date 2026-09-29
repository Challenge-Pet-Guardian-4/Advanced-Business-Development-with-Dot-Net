using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class RacaTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_NomeValido_DeveInstanciarRaca()
    {
        // Act
        var raca = new Raca("Golden Retriever");

        // Assert
        Assert.NotEqual(Guid.Empty, raca.Id);
        Assert.Equal("Golden Retriever", raca.NomeRaca);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Raca(nomeInvalido!));

        // Assert
        Assert.Equal("O nome da raça não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() => new Raca(nomeLongo));

        // Assert
        Assert.Equal("O nome da raça deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Atualizar_NomeValido_DeveAtualizarPropriedade()
    {
        // Arrange
        var raca = fixture.CriarRacaValida("Labrador");

        // Act
        raca.Atualizar("Labrador Retriever");

        // Assert
        Assert.Equal("Labrador Retriever", raca.NomeRaca);
    }
}
