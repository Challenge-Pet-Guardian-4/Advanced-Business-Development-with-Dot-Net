using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class PetTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarPetComSucesso()
    {
        // Arrange
        var nome = "Thor";
        var dataNascimento = DateTime.UtcNow.AddYears(-2);
        var sexo = SexoPet.Macho;
        var porte = PortePet.Grande;
        var castrado = false;
        var racaId = Guid.NewGuid();

        // Act
        var pet = new Pet(nome, dataNascimento, sexo, porte, castrado, racaId);

        // Assert
        Assert.NotEqual(Guid.Empty, pet.Id);
        Assert.Equal(nome, pet.Nome);
        Assert.Equal(dataNascimento.Date, pet.DataNascimento);
        Assert.Equal(sexo, pet.Sexo);
        Assert.Equal(porte, pet.Porte);
        Assert.False(pet.Castrado);
        Assert.Equal(racaId, pet.RacaId);
        Assert.Equal(2, pet.IdadeEmAnos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Pet(nomeInvalido!, DateTime.UtcNow.AddYears(-1), SexoPet.Femea, PortePet.Pequeno, false, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do pet não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_DataNascimentoNoFuturo_DeveLancarDomainException()
    {
        // Arrange
        var dataFutura = DateTime.UtcNow.AddDays(2);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Pet("Bob", dataFutura, SexoPet.Macho, PortePet.Medio, false, Guid.NewGuid()));

        // Assert
        Assert.Equal("A data de nascimento não pode estar no futuro.", ex.Message);
    }

    [Fact]
    public void Construtor_RacaIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Pet("Mel", DateTime.UtcNow.AddYears(-1), SexoPet.Femea, PortePet.Pequeno, false, Guid.Empty));

        // Assert
        Assert.Equal("O pet deve estar associado a uma raça válida.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var pet = fixture.CriarPetValido("Rex Antigo");
        var novoNome = "Rex Novo";
        var novaData = DateTime.UtcNow.AddYears(-4);
        var novoSexo = SexoPet.Femea;
        var novoPorte = PortePet.Grande;
        var novoCastrado = true;
        var novaRacaId = Guid.NewGuid();

        // Act
        pet.Atualizar(novoNome, novaData, novoSexo, novoPorte, novoCastrado, novaRacaId);

        // Assert
        Assert.Equal(novoNome, pet.Nome);
        Assert.Equal(novaData.Date, pet.DataNascimento);
        Assert.Equal(novoSexo, pet.Sexo);
        Assert.Equal(novoPorte, pet.Porte);
        Assert.True(pet.Castrado);
        Assert.Equal(novaRacaId, pet.RacaId);
    }

    [Fact]
    public void Castrar_PetNaoCastrado_DeveMarcarComoCastrado()
    {
        // Arrange
        var pet = fixture.CriarPetValido();

        // Act
        pet.Castrar();

        // Assert
        Assert.True(pet.Castrado);
    }

    [Fact]
    public void Castrar_PetJaCastrado_DeveLancarDomainException()
    {
        // Arrange
        var pet = fixture.CriarPetValido();
        pet.Castrar();

        // Act
        var ex = Assert.Throws<DomainException>(() => pet.Castrar());

        // Assert
        Assert.Equal("O pet já foi castrado.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Pet(nomeLongo, DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Pequeno, false, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do pet deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_DataNascimentoMuitoAntiga_DeveLancarDomainException()
    {
        // Arrange
        var dataAntiga = DateTime.UtcNow.AddYears(-41);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Pet("Rex", dataAntiga, SexoPet.Macho, PortePet.Pequeno, false, Guid.NewGuid()));

        // Assert
        Assert.Equal("A data de nascimento informada é inválida.", ex.Message);
    }

    [Fact]
    public void Atualizar_NomeInvalido_DeveLancarDomainException()
    {
        // Arrange
        var pet = fixture.CriarPetValido();

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            pet.Atualizar("", DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do pet não pode ser vazio.", ex.Message);
    }
}
