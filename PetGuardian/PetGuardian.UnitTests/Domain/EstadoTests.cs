using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class EstadoTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_NomeValido_DeveInstanciarEstado()
    {
        // Act
        var estado = new Estado("São Paulo");

        // Assert
        Assert.NotEqual(Guid.Empty, estado.Id);
        Assert.Equal("São Paulo", estado.NomeEstado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Estado(nomeInvalido!));

        // Assert
        Assert.Equal("O nome do estado não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() => new Estado(nomeLongo));

        // Assert
        Assert.Equal("O nome do estado deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Atualizar_NomeValido_DeveAtualizarPropriedade()
    {
        // Arrange
        var estado = fixture.CriarEstadoValido("Minas");

        // Act
        estado.Atualizar("Minas Gerais");

        // Assert
        Assert.Equal("Minas Gerais", estado.NomeEstado);
    }
}
