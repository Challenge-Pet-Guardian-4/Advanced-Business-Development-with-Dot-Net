using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class TrilhaTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarTrilha()
    {
        // Arrange
        var nome = "Trilha de Saúde";
        var descricao = "Cuidados preventivos";
        var petId = Guid.NewGuid();

        // Act
        var trilha = new Trilha(nome, descricao, petId);

        // Assert
        Assert.NotEqual(Guid.Empty, trilha.Id);
        Assert.Equal(nome, trilha.Nome);
        Assert.Equal(descricao, trilha.Descricao);
        Assert.Equal(petId, trilha.PetId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Trilha(nomeInvalido!, "Descricao valida", Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome da trilha não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Trilha(nomeLongo, "Descricao valida", Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome da trilha deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_PetIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Trilha("Nome", "Descricao", Guid.Empty));

        // Assert
        Assert.Equal("A trilha deve estar associada a um pet válido.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var trilha = fixture.CriarTrilhaValida();
        var novoNome = "Novo Nome";
        var novaDescricao = "Nova Descricao";

        // Act
        trilha.Atualizar(novoNome, novaDescricao);

        // Assert
        Assert.Equal(novoNome, trilha.Nome);
        Assert.Equal(novaDescricao, trilha.Descricao);
    }
}
