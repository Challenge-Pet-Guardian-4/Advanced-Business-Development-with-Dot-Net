using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class UsuarioTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarUsuarioComSucesso()
    {
        // Arrange
        var nome = "Ana Clara";
        var email = "ana@exemplo.com";
        var senha = "senhaForte123";
        var role = RoleUsuario.Premium;
        var telefoneId = Guid.NewGuid();

        // Act
        var usuario = new Usuario(nome, email, senha, role, telefoneId);

        // Assert
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(nome, usuario.Nome);
        Assert.Equal(email, usuario.Email);
        Assert.True(usuario.VerifyPassword(senha));
        Assert.False(usuario.VerifyPassword("senhaErrada"));
        Assert.NotEmpty(usuario.Salt);
        Assert.NotEqual(senha, usuario.Senha);
        Assert.Equal(role, usuario.Role);
        Assert.Equal(telefoneId, usuario.TelefoneId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AtualizarNome_NomeInvalido_DeveLancarDomainException(string? nomeInvalido)
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();

        // Act
        var ex = Assert.Throws<DomainException>(() => usuario.AtualizarNome(nomeInvalido!));

        // Assert
        Assert.Equal("O nome não pode ser vazio.", ex.Message);
    }

    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("")]
    [InlineData(null)]
    public void AtualizarEmail_EmailInvalido_DeveLancarDomainException(string? emailInvalido)
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();

        // Act
        var ex = Assert.Throws<DomainException>(() => usuario.AtualizarEmail(emailInvalido!));

        // Assert
        Assert.Equal("O e-mail informado é inválido.", ex.Message);
    }

    [Theory]
    [InlineData("12345")] // Menor que 6
    [InlineData("")]
    public void AtualizarSenha_SenhaCurta_DeveLancarDomainException(string senhaCurta)
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();

        // Act
        var ex = Assert.Throws<DomainException>(() => usuario.AtualizarSenha(senhaCurta));

        // Assert
        Assert.Equal("A senha deve ter pelo menos 6 caracteres.", ex.Message);
    }

    [Fact]
    public void AtualizarRole_NovaRole_DeveAtualizarPropriedade()
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();

        // Act
        usuario.AtualizarRole(RoleUsuario.Premium);

        // Assert
        Assert.Equal(RoleUsuario.Premium, usuario.Role);
    }

    [Fact]
    public void Construtor_TelefoneIdVazio_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Usuario("Nome", "email@teste.com", "senha123", RoleUsuario.Comum, Guid.Empty));

        // Assert
        Assert.Equal("O usuário deve ter um telefone válido.", ex.Message);
    }

    [Fact]
    public void AtualizarNome_NomeMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();
        var nomeLongo = new string('A', 101);

        // Act
        var ex = Assert.Throws<DomainException>(() => usuario.AtualizarNome(nomeLongo));

        // Assert
        Assert.Equal("O nome deve ter no máximo 100 caracteres.", ex.Message);
    }

    [Fact]
    public void AtualizarEmail_EmailMuitoLongo_DeveLancarDomainException()
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();
        var emailLongo = new string('a', 45) + "@email.com"; // 55 caracteres

        // Act
        var ex = Assert.Throws<DomainException>(() => usuario.AtualizarEmail(emailLongo));

        // Assert
        Assert.Equal("O e-mail deve ter no máximo 50 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void VerifyPassword_SenhaEmBranco_DeveRetornarFalse(string? senhaEmBranco)
    {
        // Arrange
        var usuario = fixture.CriarUsuarioValido();

        // Act
        var resultado = usuario.VerifyPassword(senhaEmBranco!);

        // Assert
        Assert.False(resultado);
    }
}
