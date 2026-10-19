using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class UsuarioServiceTests(TestFixture fixture)
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IRepository<Telefone>> _telefoneRepoMock = new();
    private readonly Mock<IEnderecoService> _enderecoServiceMock = new();
    private readonly Mock<ITarefaRepository> _tarefaRepoMock = new();
    private readonly Mock<ILogger<UsuarioService>> _loggerMock = new();

    private UsuarioService CreateService() =>
        new(_usuarioRepoMock.Object, _telefoneRepoMock.Object, _enderecoServiceMock.Object, _tarefaRepoMock.Object, _loggerMock.Object);

    [Fact]
    public void Create_EmailNovoEDadosValidos_DeveCriarTelefoneEnderecoEUsuario()
    {
        // Arrange
        var service = CreateService();
        var endId = Guid.NewGuid();
        var endRequest = new EnderecoRequest("01001-000", "100");
        var request = new UsuarioRequest("Lucas", "lucas@exemplo.com", "senha123", RoleUsuario.Comum, "11", "987654321", endRequest);

        _usuarioRepoMock.Setup(r => r.ExistsByEmail(request.Email)).Returns(false);
        _enderecoServiceMock.Setup(s => s.Create(endRequest))
            .Returns(new EnderecoResponse(endId, "01001000", "Praça da Sé", "100", Guid.NewGuid()));
        _telefoneRepoMock.Setup(r => r.Add(It.IsAny<Telefone>())).Returns<Telefone>(t => t);
        _usuarioRepoMock.Setup(r => r.Add(It.IsAny<Usuario>())).Returns<Usuario>(u => u);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Lucas", response.Nome);
        Assert.Equal(endId, response.EnderecoId);
        _enderecoServiceMock.Verify(s => s.Create(endRequest), Times.Once);
        _telefoneRepoMock.Verify(r => r.Add(It.IsAny<Telefone>()), Times.Once);
        _usuarioRepoMock.Verify(r => r.Add(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public void Create_EmailDuplicado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var request = new UsuarioRequest("Lucas", "duplicado@exemplo.com", "senha123", RoleUsuario.Comum, "11", "987654321", new EnderecoRequest("01001-000", "100"));

        _usuarioRepoMock.Setup(r => r.ExistsByEmail(request.Email)).Returns(true);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("Já existe um usuário com este e-mail.", ex.Message);
        _usuarioRepoMock.Verify(r => r.Add(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public void GetScore_UsuarioComTarefasConcluidas_DeveSomarPontosCorretamente()
    {
        // Arrange
        var service = CreateService();
        var usuario = fixture.CriarUsuarioValido();

        var t1 = fixture.CriarTarefaValida(usuarioId: usuario.Id);
        t1.Concluir(); // 50 pontos

        var t2 = fixture.CriarTarefaValida(usuarioId: usuario.Id);
        t2.Concluir(); // 50 pontos

        var t3 = fixture.CriarTarefaValida(usuarioId: usuario.Id); // Não concluída (0 pontos)

        _usuarioRepoMock.Setup(r => r.ExistsById(usuario.Id)).Returns(true);
        _tarefaRepoMock.Setup(r => r.GetByUsuarioId(usuario.Id)).Returns([t1, t2, t3]);

        // Act
        var score = service.GetScore(usuario.Id);

        // Assert
        Assert.Equal(100, score.PontosTotais);
    }
}
