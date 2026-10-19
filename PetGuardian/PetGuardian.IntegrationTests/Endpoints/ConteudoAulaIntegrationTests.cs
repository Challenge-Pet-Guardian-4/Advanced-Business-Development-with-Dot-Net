using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ConteudoAulaIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _admin = factory.CreateAuthenticatedClient(RoleUsuario.Admin);

    private async Task<AulaResponse> CriarAulaAsync(string nome)
    {
        var racas = await _admin.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        var petRes = await _admin.PostAsJsonAsync("/api/pet",
            new PetRequest("Pet Conteudo", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Medio, false, racas!.First().Id));
        var pet = await petRes.Content.ReadFromJsonAsync<PetResponse>();
        var trilha = await (await _admin.PostAsJsonAsync("/api/trilha", new TrilhaRequest("Trilha Conteudo", "Desc", pet!.Id)))
            .Content.ReadFromJsonAsync<TrilhaResponse>();
        var modulo = await (await _admin.PostAsJsonAsync("/api/modulo", new ModuloRequest("Modulo C", "30 min", "Desc", trilha!.Id)))
            .Content.ReadFromJsonAsync<ModuloResponse>();
        var aula = await (await _admin.PostAsJsonAsync("/api/aula", new AulaRequest(nome, "Desc", 10, "Facil", false, modulo!.Id)))
            .Content.ReadFromJsonAsync<AulaResponse>();
        return aula!;
    }

    private static ConteudoAulaRequest ConteudoValido() =>
        new("ARTIGO", "# Titulo\n\nTexto da aula.", ["https://exemplo.com/guia.pdf"]);

    [Fact]
    public async Task Post_AdminComAulaExistente_DeveRetornar201ComLinks()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Post");

        // Act
        var response = await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var conteudo = await response.Content.ReadFromJsonAsync<ConteudoAulaResponse>();
        Assert.Equal(aula.Id, conteudo!.AulaId);
        Assert.Equal("ARTIGO", conteudo.TipoConteudo);
        Assert.Contains(conteudo.Links!, l => l.Rel == "self");
    }

    [Fact]
    public async Task Post_UsuarioComum_DeveRetornar403Forbidden()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo 403");
        var comum = factory.CreateAuthenticatedClient(RoleUsuario.Comum);

        // Act
        var response = await comum.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_AulaInexistente_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _admin.PostAsJsonAsync($"/api/aula/{Guid.NewGuid()}/conteudo", ConteudoValido());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ConteudoDuplicado_DeveRetornar400BadRequest()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Dup");
        await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Act
        var response = await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_CorpoInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Invalido");

        // Act
        var response = await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", new ConteudoAulaRequest("ARTIGO", "", null));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ConteudoExistente_DeveRetornar200OkEEnriquecerAula()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Get");
        await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Act
        var response = await _admin.GetAsync($"/api/aula/{aula.Id}/conteudo");
        var aulaGetResponse = await _admin.GetAsync($"/api/aula/{aula.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var conteudo = await response.Content.ReadFromJsonAsync<ConteudoAulaResponse>();
        Assert.Equal("# Titulo\n\nTexto da aula.", conteudo!.CorpoMarkdown);

        Assert.Equal(HttpStatusCode.OK, aulaGetResponse.StatusCode);
        var aulaEnriquecida = await aulaGetResponse.Content.ReadFromJsonAsync<AulaResponse>();
        Assert.Equal("# Titulo\n\nTexto da aula.", aulaEnriquecida!.Conteudo);
    }

    [Fact]
    public async Task Get_AulaSemConteudo_DeveRetornar404NotFound()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Sem Conteudo");

        // Act
        var response = await _admin.GetAsync($"/api/aula/{aula.Id}/conteudo");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ConteudoExistente_DeveAtualizarERetornar200Ok()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Put");
        await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Act
        var response = await _admin.PutAsJsonAsync($"/api/aula/{aula.Id}/conteudo",
            new ConteudoAulaRequest("VIDEO", "# Novo corpo", []));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var conteudo = await response.Content.ReadFromJsonAsync<ConteudoAulaResponse>();
        Assert.Equal("VIDEO", conteudo!.TipoConteudo);
        Assert.Equal("# Novo corpo", conteudo.CorpoMarkdown);
    }

    [Fact]
    public async Task Put_ConteudoInexistente_DeveRetornar404NotFound()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Put 404");

        // Act
        var response = await _admin.PutAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ConteudoExistente_DeveRetornar204NoContent()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Conteudo Del");
        await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Act
        var response = await _admin.DeleteAsync($"/api/aula/{aula.Id}/conteudo");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _admin.GetAsync($"/api/aula/{aula.Id}/conteudo")).StatusCode);
    }

    [Fact]
    public async Task DeleteAula_ComConteudo_DeveRemoverTambemOConteudo()
    {
        // Arrange
        var aula = await CriarAulaAsync("Aula Cascata Conteudo");
        await _admin.PostAsJsonAsync($"/api/aula/{aula.Id}/conteudo", ConteudoValido());

        // Act
        var delete = await _admin.DeleteAsync($"/api/aula/{aula.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _admin.GetAsync($"/api/aula/{aula.Id}/conteudo")).StatusCode);
    }
}
