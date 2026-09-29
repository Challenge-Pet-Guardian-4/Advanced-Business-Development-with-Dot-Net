using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class UsuarioEnderecoTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarUsuarioEndereco()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var endId = Guid.NewGuid();

        // Act
        var vinculo = fixture.CriarUsuarioEnderecoValido(userId, endId);

        // Assert
        Assert.Equal(userId, vinculo.UsuarioId);
        Assert.Equal(endId, vinculo.EnderecoId);
    }

    [Fact]
    public void Construtor_UsuarioIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new UsuarioEndereco(Guid.Empty, Guid.NewGuid()));

        // Assert
        Assert.Equal("O vínculo deve ter um usuário válido.", ex.Message);
    }

    [Fact]
    public void Construtor_EnderecoIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new UsuarioEndereco(Guid.NewGuid(), Guid.Empty));

        // Assert
        Assert.Equal("O vínculo deve ter um endereço válido.", ex.Message);
    }
}
