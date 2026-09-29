using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class StatusTests(TestFixture fixture)
{
    [Theory]
    [InlineData("PENDENTE")]
    [InlineData("CONCLUIDO")]
    [InlineData("EXPIRADO")]
    [InlineData("pendente")] // Deve normalizar para ToUpperInvariant
    public void Construtor_ValoresValidos_DeveInstanciarStatus(string statusValido)
    {
        // Act
        var status = new Status(statusValido);

        // Assert
        Assert.NotEqual(Guid.Empty, status.Id);
        Assert.Equal(statusValido.Trim().ToUpperInvariant(), status.NomeStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_NomeVazio_DeveLancarDomainException(string? nomeVazio)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Status(nomeVazio!));

        // Assert
        Assert.Equal("O nome do status não pode ser vazio.", ex.Message);
    }

    [Theory]
    [InlineData("CANCELADO")]
    [InlineData("EM_ANDAMENTO")]
    [InlineData("INEXISTENTE")]
    public void Construtor_ValorNaoPermitido_DeveLancarDomainException(string valorInvalido)
    {
        // Act
        var ex = Assert.Throws<DomainException>(() => new Status(valorInvalido));

        // Assert
        Assert.Equal("Status inválido. Valores aceitos: CONCLUIDO, EXPIRADO, PENDENTE.", ex.Message);
    }

    [Fact]
    public void Atualizar_ValorValido_DeveAtualizarPropriedade()
    {
        // Arrange
        var status = fixture.CriarStatusValido("PENDENTE");

        // Act
        status.Atualizar("CONCLUIDO");

        // Assert
        Assert.Equal("CONCLUIDO", status.NomeStatus);
    }
}
