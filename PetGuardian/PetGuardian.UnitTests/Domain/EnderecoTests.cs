using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class EnderecoTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarEndereco()
    {
        // Arrange
        var cep = "01001-000";
        var rua = "Praça da Sé";
        var numero = "100";
        var bairroId = Guid.NewGuid();

        // Act
        var end = new Endereco(cep, rua, numero, bairroId);

        // Assert
        Assert.NotEqual(Guid.Empty, end.Id);
        Assert.Equal("01001000", end.Cep); // Máscara é limpa
        Assert.Equal(rua, end.Rua);
        Assert.Equal(numero, end.Numero);
        Assert.Equal(bairroId, end.BairroId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_CepVazio_DeveLancarDomainException(string? cepInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Endereco(cepInvalido!, "Rua", "10", Guid.NewGuid()));

        // Assert
        Assert.Equal("O CEP não pode ser vazio.", ex.Message);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789")]
    public void Construtor_CepTamanhoInvalido_DeveLancarDomainException(string cepInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Endereco(cepInvalido, "Rua", "10", Guid.NewGuid()));

        // Assert
        Assert.Equal("O CEP deve ter 8 dígitos.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_RuaVazia_DeveLancarDomainException(string? ruaInvalida)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Endereco("01001000", ruaInvalida!, "10", Guid.NewGuid()));

        // Assert
        Assert.Equal("A rua não pode ser vazia.", ex.Message);
    }

    [Fact]
    public void Construtor_NumeroMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var numeroLongo = "123456"; // 6 caracteres

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Endereco("01001000", "Rua", numeroLongo, Guid.NewGuid()));

        // Assert
        Assert.Equal("O número deve ter no máximo 5 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_BairroIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Endereco("01001000", "Rua", "10", Guid.Empty));

        // Assert
        Assert.Equal("O endereço deve estar associado a um bairro válido.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var end = fixture.CriarEnderecoValido();
        var novoCep = "04567-000";
        var novaRua = "Av Paulista";
        var novoNum = "500";
        var novoBairroId = Guid.NewGuid();

        // Act
        end.Atualizar(novoCep, novaRua, novoNum, novoBairroId);

        // Assert
        Assert.Equal("04567000", end.Cep);
        Assert.Equal(novaRua, end.Rua);
        Assert.Equal(novoNum, end.Numero);
        Assert.Equal(novoBairroId, end.BairroId);
    }
}
