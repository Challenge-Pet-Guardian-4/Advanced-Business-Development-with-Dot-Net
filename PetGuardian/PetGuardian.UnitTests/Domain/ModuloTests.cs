using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class ModuloTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarModulo()
    {
        // Arrange
        var nome = "Módulo 1";
        var tempo = "2 horas";
        var descricao = "Comandos básicos";
        var trilhaId = Guid.NewGuid();

        // Act
        var modulo = new Modulo(nome, tempo, descricao, trilhaId);

        // Assert
        Assert.NotEqual(Guid.Empty, modulo.Id);
        Assert.Equal(nome, modulo.Nome);
        Assert.Equal(tempo, modulo.TempoConclusao);
        Assert.Equal(descricao, modulo.Descricao);
        Assert.Equal(trilhaId, modulo.TrilhaId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Modulo(nomeInvalido!, "1h", "Descricao", Guid.NewGuid()));

        // Assert
        Assert.Equal("O nome do módulo não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_TrilhaIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Modulo("Modulo 1", "1h", "Descricao", Guid.Empty));

        // Assert
        Assert.Equal("O módulo deve estar associado a uma trilha válida.", ex.Message);
    }

    [Fact]
    public void Construtor_TempoConclusaoMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var tempoLongo = "12345678901"; // 11 caracteres

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Modulo("Modulo 1", tempoLongo, "Descricao", Guid.NewGuid()));

        // Assert
        Assert.Equal("O tempo de conclusão deve ter no máximo 10 caracteres.", ex.Message);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var modulo = fixture.CriarModuloValido();
        var novoNome = "Modulo Atualizado";
        var novoTempo = "3 horas";
        var novaDesc = "Nova Descricao";

        // Act
        modulo.Atualizar(novoNome, novoTempo, novaDesc);

        // Assert
        Assert.Equal(novoNome, modulo.Nome);
        Assert.Equal(novoTempo, modulo.TempoConclusao);
        Assert.Equal(novaDesc, modulo.Descricao);
    }
}
