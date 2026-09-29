using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class CidadeTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarCidade()
    {
        // Arrange
        var estadoId = Guid.NewGuid();

        // Act
        var cidade = new Cidade("Campinas", estadoId);

        // Assert
        Assert.NotEqual(Guid.Empty, cidade.Id);
        Assert.Equal("Campinas", cidade.NomeCidade);
        Assert.Equal(estadoId, cidade.EstadoId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Cidade(nomeInvalido!, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome da cidade não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() => new Cidade(nomeLongo, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome da cidade deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_EstadoIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Cidade("Campinas", Guid.Empty));

        // Assert
        Assert.Equal("A cidade deve estar associada a um estado válido.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var cidade = fixture.CriarCidadeValida("Campinas");
        var novoEstadoId = Guid.NewGuid();

        // Act
        cidade.Atualizar("Campinas Atualizada", novoEstadoId);

        // Assert
        Assert.Equal("Campinas Atualizada", cidade.NomeCidade);
        Assert.Equal(novoEstadoId, cidade.EstadoId);
    }
}
