using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using Xunit;

namespace PetGuardian.UnitTests.Application;

public class ConteudoAulaServiceTests
{
    private readonly Mock<IAulaRepository> _aulaRepoMock = new();
    private readonly Mock<IConteudoAulaRepository> _conteudoRepoMock = new();
    private readonly Mock<ILogger<ConteudoAulaService>> _loggerMock = new();

    private ConteudoAulaService CreateService() =>
        new(_aulaRepoMock.Object, _conteudoRepoMock.Object, _loggerMock.Object);

    private static ConteudoAulaRequest RequestValido() =>
        new("ARTIGO", "# Titulo", ["https://exemplo.com/guia.pdf"]);

    [Fact]
    public async Task CreateAsync_AulaExistenteSemConteudo_DeveInserirDocumento()
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(true);
        _conteudoRepoMock.Setup(r => r.GetByAulaIdAsync(aulaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConteudoAulaResponse?)null);
        _conteudoRepoMock.Setup(r => r.InserirAsync(aulaId, It.IsAny<ConteudoAulaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, ConteudoAulaRequest req, CancellationToken _) =>
                new ConteudoAulaResponse("mongo-1", id, req.TipoConteudo, req.CorpoMarkdown, req.LinksRecursos ?? [], DateTime.UtcNow));

        // Act
        var resultado = await service.CreateAsync(aulaId, RequestValido());

        // Assert
        Assert.Equal("mongo-1", resultado.Id);
        Assert.Equal(aulaId, resultado.AulaId);
        Assert.Equal("ARTIGO", resultado.TipoConteudo);
        Assert.Equal("# Titulo", resultado.CorpoMarkdown);
    }

    [Fact]
    public async Task CreateAsync_AulaInexistente_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(false);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(aulaId, RequestValido()));

        // Assert
        Assert.Equal("Aula não encontrada.", ex.Message);
        _conteudoRepoMock.Verify(r => r.InserirAsync(It.IsAny<Guid>(), It.IsAny<ConteudoAulaRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_AulaJaPossuiConteudo_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(true);
        _conteudoRepoMock.Setup(r => r.GetByAulaIdAsync(aulaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConteudoAulaResponse("x", aulaId, "ARTIGO", "# a", [], DateTime.UtcNow));

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(aulaId, RequestValido()));

        // Assert
        Assert.Equal("A aula já possui conteúdo cadastrado.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ConteudoExistente_DeveRetornarAtualizado()
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(true);
        _conteudoRepoMock.Setup(r => r.AtualizarAsync(aulaId, It.IsAny<ConteudoAulaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, ConteudoAulaRequest req, CancellationToken _) =>
                new ConteudoAulaResponse("mongo-1", id, req.TipoConteudo, req.CorpoMarkdown, req.LinksRecursos ?? [], DateTime.UtcNow));

        // Act
        var resultado = await service.UpdateAsync(aulaId, RequestValido());

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("mongo-1", resultado.Id);
    }

    [Fact]
    public async Task UpdateAsync_ConteudoInexistente_DeveRetornarNull()
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(true);
        _conteudoRepoMock.Setup(r => r.AtualizarAsync(aulaId, It.IsAny<ConteudoAulaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConteudoAulaResponse?)null);

        // Act
        var resultado = await service.UpdateAsync(aulaId, RequestValido());

        // Assert
        Assert.Null(resultado);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_DeveRefletirResultadoDoRepositorio(bool removido)
    {
        // Arrange
        var service = CreateService();
        var aulaId = Guid.NewGuid();
        _aulaRepoMock.Setup(r => r.ExistsById(aulaId)).Returns(true);
        _conteudoRepoMock.Setup(r => r.ExcluirPorAulaIdAsync(aulaId, It.IsAny<CancellationToken>())).ReturnsAsync(removido);

        // Act
        var resultado = await service.DeleteAsync(aulaId);

        // Assert
        Assert.Equal(removido, resultado);
    }
}
