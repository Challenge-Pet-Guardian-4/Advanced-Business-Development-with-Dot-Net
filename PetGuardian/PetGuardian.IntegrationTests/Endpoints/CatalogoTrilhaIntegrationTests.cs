using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class CatalogoTrilhaIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _admin = factory.CreateAuthenticatedClient(RoleUsuario.Admin);

    private async Task<TrilhaResponse> CriarTrilhaComModuloEAulaAsync(string nomeTrilha)
    {
        var racas = await _admin.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        var petRes = await _admin.PostAsJsonAsync("/api/pet",
            new PetRequest("Pet Catalogo", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Medio, false, racas!.First().Id));
        var pet = await petRes.Content.ReadFromJsonAsync<PetResponse>();

        var trilha = await (await _admin.PostAsJsonAsync("/api/trilha", new TrilhaRequest(nomeTrilha, "Desc trilha", pet!.Id)))
            .Content.ReadFromJsonAsync<TrilhaResponse>();
        var modulo = await (await _admin.PostAsJsonAsync("/api/modulo", new ModuloRequest("Modulo 1", "30 min", "Desc", trilha!.Id)))
            .Content.ReadFromJsonAsync<ModuloResponse>();
        await _admin.PostAsJsonAsync("/api/aula",
            new AulaRequest("Aula 1", "Desc aula", 20, "Facil", "Conteudo da aula", false, modulo!.Id));

        return trilha;
    }

    [Fact]
    public async Task Sincronizar_Admin_DeveGravarDocumentoComModulosEAulasEmbutidos()
    {
        // Arrange
        var trilha = await CriarTrilhaComModuloEAulaAsync("Trilha Mongo Sync");

        // Act
        var sync = await _admin.PostAsync("/api/catalogo/trilhas/sincronizar", null);
        var doc = await _admin.GetAsync($"/api/catalogo/trilhas/{trilha.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, sync.StatusCode);
        var resumo = await sync.Content.ReadFromJsonAsync<SincronizacaoCatalogoResponse>();
        Assert.True(resumo!.TrilhasRelacionais >= 1);

        Assert.Equal(HttpStatusCode.OK, doc.StatusCode);
        var catalogo = await doc.Content.ReadFromJsonAsync<TrilhaCatalogoResponse>();
        Assert.Equal("Trilha Mongo Sync", catalogo!.Nome);
        var modulo = Assert.Single(catalogo.Modulos);
        var aula = Assert.Single(modulo.Aulas);
        Assert.Equal("Aula 1", aula.Titulo);
        Assert.Contains(catalogo.Links!, l => l.Rel == "self");
    }

    [Fact]
    public async Task Sincronizar_UsuarioComum_DeveRetornar403Forbidden()
    {
        // Arrange
        var comum = factory.CreateAuthenticatedClient(RoleUsuario.Comum);

        // Act
        var response = await comum.PostAsync("/api/catalogo/trilhas/sincronizar", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCatalogo_AposSincronizar_DeveBuscarPorTermoComPaginacao()
    {
        // Arrange
        await CriarTrilhaComModuloEAulaAsync("Trilha Busca Termo Unico");
        await _admin.PostAsync("/api/catalogo/trilhas/sincronizar", null);

        // Act
        var pagina = await _admin.GetFromJsonAsync<PagedResponse<TrilhaCatalogoResponse>>(
            "/api/catalogo/trilhas?termo=busca termo unico&pageSize=5");

        // Assert
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Items, t => t.Nome == "Trilha Busca Termo Unico");
        Assert.NotNull(pagina.Links);
    }

    [Fact]
    public async Task GetCatalogo_TrilhaNaoSincronizada_DeveRetornar404NotFound()
    {
        // Act
        var response = await _admin.GetAsync($"/api/catalogo/trilhas/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}