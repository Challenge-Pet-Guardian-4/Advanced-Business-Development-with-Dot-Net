using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class TelefoneTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarTelefoneEFormatarCompleto()
    {
        // Act
        var tel = new Telefone("11", "987654321");

        // Assert
        Assert.NotEqual(Guid.Empty, tel.Id);
        Assert.Equal("11", tel.NumDdd);
        Assert.Equal("987654321", tel.NumTel);
        Assert.Equal("(11) 987654321", tel.Completo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData(null)]
    public void Construtor_DddInvalido_DeveLancarDomainException(string? dddInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Telefone(dddInvalido!, "987654321"));

        // Assert
        Assert.Equal("O DDD deve ter exatamente 2 dígitos.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567890")] // 10 dígitos (máximo 9)
    [InlineData(null)]
    public void Construtor_NumeroInvalido_DeveLancarDomainException(string? numeroInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Telefone("11", numeroInvalido!));

        // Assert
        Assert.Equal("O número de telefone deve ter no máximo 9 dígitos.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var tel = fixture.CriarTelefoneValido("11", "911112222");

        // Act
        tel.Atualizar("19", "933334444");

        // Assert
        Assert.Equal("19", tel.NumDdd);
        Assert.Equal("933334444", tel.NumTel);
        Assert.Equal("(19) 933334444", tel.Completo);
    }
}
