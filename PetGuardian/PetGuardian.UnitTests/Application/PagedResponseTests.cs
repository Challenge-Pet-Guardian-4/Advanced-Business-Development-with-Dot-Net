using PetGuardian.Application.Common;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Application;

[Collection(UnitTestCollection.Name)]
public class PagedResponseTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(101, 25, 5)]
    public void Create_TotalDeRegistros_DeveCalcularTotalDePaginas(int total, int pageSize, int paginasEsperadas)
    {
        // Arrange
        var resultado = new PagedResult<int>([], total);

        // Act
        var resposta = PagedResponse<int>.Create(resultado, new PageQuery { PageSize = pageSize });

        // Assert
        Assert.Equal(paginasEsperadas, resposta.TotalPages);
        Assert.Equal(total, resposta.TotalCount);
    }

    [Fact]
    public void Normalized_ValoresForaDoLimite_DeveSanearPageEPageSize()
    {
        // Arrange
        var query = new PageQuery { Page = -3, PageSize = 5000, SortBy = "  Nome ", SortDir = "xyz" };

        // Act
        var normalizada = query.Normalized();

        // Assert
        Assert.Equal(1, normalizada.Page);
        Assert.Equal(PageQuery.MaxPageSize, normalizada.PageSize);
        Assert.Equal("Nome", normalizada.SortBy);
        Assert.Equal("asc", normalizada.SortDir);
    }
}