using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class AulaTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarAula()
    {
        // Arrange
        var nome = "Aula 1: Sentar";
        var desc = "Treino básico";
        var pontos = 20;
        var dif = "Facil";
        var conteudo = "Conteudo de treino";
        var moduloId = Guid.NewGuid();

        // Act
        var aula = new Aula(nome, desc, pontos, dif, conteudo, false, moduloId);

        // Assert
        Assert.NotEqual(Guid.Empty, aula.Id);
        Assert.Equal(nome, aula.Nome);
        Assert.Equal(desc, aula.Descricao);
        Assert.Equal(pontos, aula.PontosAula);
        Assert.Equal(dif, aula.Dificuldade);
        Assert.Equal(conteudo, aula.Conteudo);
        Assert.False(aula.Concluida);
        Assert.Equal(moduloId, aula.ModuloId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Aula(nomeInvalido!, "Desc", 10, "Facil", "Texto", false, Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome da aula não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_PontosNegativos_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Aula("Aula 1", "Desc", -5, "Facil", "Texto", false, Guid.NewGuid()));

        // Assert
        Assert.Equal("Os pontos da aula não podem ser negativos.", ex.Message);
    }

    [Fact]
    public void Construtor_ModuloIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Aula("Aula 1", "Desc", 10, "Facil", "Texto", false, Guid.Empty));

        // Assert
        Assert.Equal("A aula deve estar associada a um módulo válido.", ex.Message);
    }

    [Fact]
    public void Concluir_AulaNaoConcluida_DeveMarcarComoConcluida()
    {
        // Arrange
        var aula = fixture.CriarAulaValida();
        Assert.False(aula.Concluida);

        // Act
        aula.Concluir();

        // Assert
        Assert.True(aula.Concluida);
    }

    [Fact]
    public void Concluir_AulaJaConcluida_DeveLancarDomainException()
    {
        // Arrange
        var aula = fixture.CriarAulaValida();
        aula.Concluir();

        // Act
        var ex = Assert.Throws<DomainException>(() => aula.Concluir());

        // Assert
        Assert.Equal("Esta aula já foi concluída.", ex.Message);
    }
}
