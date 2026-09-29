using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class UsuarioPetTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarUsuarioPet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        // Act
        var vinculo = new UsuarioPet(userId, petId, true);

        // Assert
        Assert.Equal(userId, vinculo.UsuarioId);
        Assert.Equal(petId, vinculo.PetId);
        Assert.True(vinculo.ResponPrinc);
    }

    [Fact]
    public void Construtor_UsuarioIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new UsuarioPet(Guid.Empty, Guid.NewGuid(), true));

        // Assert
        Assert.Equal("O vínculo deve ter um usuário válido.", ex.Message);
    }

    [Fact]
    public void Construtor_PetIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new UsuarioPet(Guid.NewGuid(), Guid.Empty, true));

        // Assert
        Assert.Equal("O vínculo deve ter um pet válido.", ex.Message);
    }

    [Fact]
    public void AtualizarResponsabilidade_ResponsavelPrincipalAtivo_DeveAlternarFlag()
    {
        // Arrange
        var vinculo = fixture.CriarUsuarioPetValido(responPrinc: false);
        Assert.False(vinculo.ResponPrinc);

        // Act
        vinculo.AtualizarResponsabilidade(true);

        // Assert
        Assert.True(vinculo.ResponPrinc);
    }
}
