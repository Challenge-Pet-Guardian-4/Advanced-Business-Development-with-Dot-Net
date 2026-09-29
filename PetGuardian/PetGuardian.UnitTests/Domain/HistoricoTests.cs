using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class HistoricoTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarHistorico()
    {
        // Arrange
        var tipo = "VACINACAO_RAIVA";
        var data = DateTime.UtcNow;
        var petId = Guid.NewGuid();

        // Act
        var hist = new Historico(tipo, data, petId);

        // Assert
        Assert.NotEqual(Guid.Empty, hist.Id);
        Assert.Equal(tipo, hist.TipoHist);
        Assert.Equal(data, hist.DataHist);
        Assert.Equal(petId, hist.PetId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_TipoInvalido_DeveLancarDomainException(string? tipoInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Historico(tipoInvalido!, DateTime.UtcNow, Guid.NewGuid()));

        // Assert
        Assert.Equal("O tipo do histórico não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void Construtor_TipoMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var tipoLongo = new string('A', 31);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Historico(tipoLongo, DateTime.UtcNow, Guid.NewGuid()));

        // Assert
        Assert.Equal("O tipo do histórico deve ter no máximo 30 caracteres.", ex.Message);
    }

    [Fact]
    public void Construtor_PetIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Historico("CONSULTA", DateTime.UtcNow, Guid.Empty));

        // Assert
        Assert.Equal("O histórico deve estar associado a um pet válido.", ex.Message);
    }

    [Fact]
    public void Registrar_DadosValidos_DeveCriarInstanciaComDataUtc()
    {
        // Arrange
        var petId = Guid.NewGuid();

        // Act
        var hist = Historico.Registrar("VACINA_APLICADA", petId);

        // Assert
        Assert.Equal("VACINA_APLICADA", hist.TipoHist);
        Assert.Equal(petId, hist.PetId);
        Assert.True((DateTime.UtcNow - hist.DataHist).TotalSeconds < 5);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarTipoEData()
    {
        // Arrange
        var hist = fixture.CriarHistoricoValido();
        var novoTipo = "CHECKUP_CONCLUIDO";
        var novaData = DateTime.UtcNow.AddDays(-1);

        // Act
        hist.Atualizar(novoTipo, novaData);

        // Assert
        Assert.Equal(novoTipo, hist.TipoHist);
        Assert.Equal(novaData, hist.DataHist);
    }
}
