using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class BairroTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarBairro()
    {
        // Arrange
        var cidadeId = Guid.NewGuid();

        // Act
        var bairro = new Bairro("Vila Madalena", cidadeId);

        // Assert
        Assert.NotEqual(Guid.Empty, bairro.Id);
        Assert.Equal("Vila Madalena", bairro.NomeBairro);
        Assert.Equal(cidadeId, bairro.CidadeId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Bairro(nomeInvalido!, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do bairro não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() => new Bairro(nomeLongo, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do bairro deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_CidadeIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Bairro("Bairro", Guid.Empty));

        // Assert
        Assert.Equal("O bairro deve estar associado a uma cidade válida.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var bairro = fixture.CriarBairroValido("Moema");
        var novaCidadeId = Guid.NewGuid();

        // Act
        bairro.Atualizar("Moema Pássaros", novaCidadeId);

        // Assert
        Assert.Equal("Moema Pássaros", bairro.NomeBairro);
        Assert.Equal(novaCidadeId, bairro.CidadeId);
    }
}
