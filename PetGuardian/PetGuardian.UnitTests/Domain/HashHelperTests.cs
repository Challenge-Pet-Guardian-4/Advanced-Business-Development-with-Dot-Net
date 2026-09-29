using PetGuardian.Domain.Helpers;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class HashHelperTests
{
    [Fact]
    public void Hash_SenhaValidaESalt_DeveGerarHashBCryptValido()
    {
        // Arrange
        var senha = "senhaForte@123";
        var salt = Guid.NewGuid().ToString("N");

        // Act
        var hash = HashHelper.Hash(senha, salt);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.StartsWith("$2", hash); // Prefixo BCrypt padrão
        Assert.NotEqual(senha, hash);
    }

    [Fact]
    public void Verify_SenhaCorreta_DeveRetornarTrue()
    {
        // Arrange
        var senha = "minhaSenhaSegura";
        var salt = Guid.NewGuid().ToString("N");
        var hash = HashHelper.Hash(senha, salt);

        // Act
        var valido = HashHelper.Verify(senha, salt, hash);

        // Assert
        Assert.True(valido);
    }

    [Fact]
    public void Verify_SenhaIncorreta_DeveRetornarFalse()
    {
        // Arrange
        var senhaCorreta = "minhaSenhaSegura";
        var senhaIncorreta = "senhaErrada";
        var salt = Guid.NewGuid().ToString("N");
        var hash = HashHelper.Hash(senhaCorreta, salt);

        // Act
        var valido = HashHelper.Verify(senhaIncorreta, salt, hash);

        // Assert
        Assert.False(valido);
    }
}
