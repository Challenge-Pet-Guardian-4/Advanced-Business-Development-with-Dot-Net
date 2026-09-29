using Microsoft.Extensions.Logging;
using Moq;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class EnderecoServiceTests(TestFixture fixture)
{
    private readonly Mock<IRepository<Endereco>> _enderecoRepoMock = new();
    private readonly Mock<IRepository<Bairro>> _bairroRepoMock = new();
    private readonly Mock<IRepository<Cidade>> _cidadeRepoMock = new();
    private readonly Mock<IRepository<Estado>> _estadoRepoMock = new();
    private readonly Mock<IViaCepService> _viaCepMock = new();
    private readonly Mock<ILogger<EnderecoService>> _loggerMock = new();

    private EnderecoService CreateService() =>
        new(_enderecoRepoMock.Object,
            _bairroRepoMock.Object,
            _cidadeRepoMock.Object,
            _estadoRepoMock.Object,
            _viaCepMock.Object,
            _loggerMock.Object);

    [Fact]
    public void Create_CepValido_DeveResolverViaCepEPersistirEndereco()
    {
        // Arrange
        var service = CreateService();
        var request = new EnderecoRequest("01001-000", "100");
        var viaCepDto = new ViaCepResponseDto(
            Cep: "01001-000",
            Logradouro: "Praça da Sé",
            Complemento: "lado ímpar",
            Bairro: "Sé",
            Localidade: "São Paulo",
            Uf: "SP",
            Estado: "São Paulo",
            Erro: null
        );

        _viaCepMock.Setup(v => v.ConsultarCepAsync("01001000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viaCepDto);

        _estadoRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Estado, bool>>>())).Returns((Estado?)null);
        _cidadeRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Cidade, bool>>>())).Returns((Cidade?)null);
        _bairroRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Bairro, bool>>>())).Returns((Bairro?)null);
        _enderecoRepoMock.Setup(r => r.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<Endereco, bool>>>())).Returns((Endereco?)null);
        _enderecoRepoMock.Setup(r => r.Add(It.IsAny<Endereco>())).Returns<Endereco>(e => e);

        // Act
        var response = service.Create(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("01001000", response.Cep);
        Assert.Equal("100", response.Numero);
        Assert.Equal("Praça da Sé", response.Rua);
        _viaCepMock.Verify(v => v.ConsultarCepAsync("01001000", It.IsAny<CancellationToken>()), Times.Once);
        _enderecoRepoMock.Verify(r => r.Add(It.IsAny<Endereco>()), Times.Once);
    }

    [Fact]
    public void Create_CepNaoEncontrado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var request = new EnderecoRequest("99999-999", "1");

        _viaCepMock.Setup(v => v.ConsultarCepAsync("99999999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ViaCepResponseDto?)null);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Create(request));

        // Assert
        Assert.Equal("CEP 99999999 não encontrado.", ex.Message);
        _enderecoRepoMock.Verify(r => r.Add(It.IsAny<Endereco>()), Times.Never);
    }

    [Fact]
    public void GetById_EnderecoExistente_DeveRetornarResponse()
    {
        // Arrange
        var service = CreateService();
        var endereco = fixture.CriarEnderecoValido();
        _enderecoRepoMock.Setup(r => r.GetById(endereco.Id)).Returns(endereco);

        // Act
        var response = service.GetById(endereco.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(endereco.Id, response.Id);
        Assert.Equal(endereco.Cep, response.Cep);
        _enderecoRepoMock.Verify(r => r.GetById(endereco.Id), Times.Once);
    }

    [Fact]
    public void Delete_IdValido_DeveRetornarTrue()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();
        _enderecoRepoMock.Setup(r => r.Delete(id)).Returns(true);

        // Act
        var resultado = service.Delete(id);

        // Assert
        Assert.True(resultado);
        _enderecoRepoMock.Verify(r => r.Delete(id), Times.Once);
    }
}
