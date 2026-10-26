using System.Net;
using System.Net.Http.Json;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class TrilhaModuloAulaPaginacaoIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _admin = factory.CreateAuthenticatedClient();

    private static string Prefixo() => $"Pg{Guid.NewGuid():N}"[..10];

    private async Task<Guid> CriarPetAsync()
    {
        var racas = await _admin.GetFromJsonAsync<List<RacaResponse>>("/api/raca");
        var res = await _admin.PostAsJsonAsync("/api/pet",
            new PetRequest(Prefixo(), DateTime.UtcNow.AddYears(-2), SexoPet.Macho, PortePet.Medio, false, racas![0].Id));
        return (await res.Content.ReadFromJsonAsync<PetResponse>())!.Id;
    }

    private async Task<(string Prefixo, Guid PetId)> CriarTresTrilhasAsync()
    {
        var petId = await CriarPetAsync();
        var prefixo = Prefixo();
        foreach (var sufixo in new[] { "A", "B", "C" })
        {
            var res = await _admin.PostAsJsonAsync("/api/trilha", new TrilhaRequest(prefixo + sufixo, "Desc", petId));
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }
        return (prefixo, petId);
    }

    private async Task<Guid> CriarModuloAsync(Guid trilhaId, string nome)
    {
        var res = await _admin.PostAsJsonAsync("/api/modulo", new ModuloRequest(nome, "30 min", "Desc", trilhaId));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<ModuloResponse>())!.Id;
    }

    private async Task<Guid> CriarAulaAsync(Guid moduloId, string nome, bool concluida = false)
    {
        var res = await _admin.PostAsJsonAsync("/api/aula", new AulaRequest(nome, "Desc", 10, "Facil", concluida, moduloId));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<AulaResponse>())!.Id;
    }

    private async Task<(Guid TrilhaId, Guid ModuloId)> CriarTrilhaEModuloAsync()
    {
        var petId = await CriarPetAsync();
        var trilha = await (await _admin.PostAsJsonAsync("/api/trilha", new TrilhaRequest(Prefixo(), "Desc", petId)))
            .Content.ReadFromJsonAsync<TrilhaResponse>();
        var moduloId = await CriarModuloAsync(trilha!.Id, Prefixo());
        return (trilha.Id, moduloId);
    }

    [Fact]
    public async Task GetTrilhas_PaginacaoEOrdenacaoDesc_DeveRetornarPaginaComTotaisELinks()
    {
        // Arrange
        var (prefixo, _) = await CriarTresTrilhasAsync();

        // Act
        var response = await _admin.GetAsync($"/api/trilha?nome={prefixo}&page=1&pageSize=2&sortBy=Nome&sortDir=desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<TrilhaResponse>>();
        Assert.NotNull(pagina);
        Assert.Equal(2, pagina.Items.Count);
        Assert.Equal(3, pagina.TotalCount);
        Assert.Equal(2, pagina.TotalPages);
        Assert.EndsWith("C", pagina.Items[0].Nome);
        Assert.Contains(pagina.Links!, l => l.Rel == "next");
        Assert.All(pagina.Items, i => Assert.Contains(i.Links!, l => l.Rel == "modulos"));
    }

    [Fact]
    public async Task GetTrilhas_FiltroPorPetId_DeveRetornarSomenteDoPet()
    {
        // Arrange
        var (_, petId) = await CriarTresTrilhasAsync();

        // Act
        var pagina = await _admin.GetFromJsonAsync<PagedResponse<TrilhaResponse>>($"/api/trilha?petId={petId}");

        // Assert
        Assert.Equal(3, pagina!.TotalCount);
        Assert.All(pagina.Items, t => Assert.Equal(petId, t.PetId));
    }

    [Fact]
    public async Task GetTrilhas_SortByInvalido_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _admin.GetAsync("/api/trilha?sortBy=CampoQueNaoExiste");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetModulos_FiltroPorTrilha_DeveRetornarModulosComLinks()
    {
        // Arrange
        var (trilhaId, _) = await CriarTrilhaEModuloAsync();
        await CriarModuloAsync(trilhaId, Prefixo());

        // Act
        var pagina = await _admin.GetFromJsonAsync<PagedResponse<ModuloResponse>>($"/api/modulo?trilhaId={trilhaId}&pageSize=10");

        // Assert
        Assert.Equal(2, pagina!.TotalCount);
        Assert.All(pagina.Items, m =>
        {
            Assert.Equal(trilhaId, m.TrilhaId);
            Assert.Contains(m.Links!, l => l.Rel == "aulas");
        });
    }

    [Fact]
    public async Task GetAulas_FiltroPorConcluida_DeveRetornarSomenteConcluidas()
    {
        // Arrange
        var (_, moduloId) = await CriarTrilhaEModuloAsync();
        await CriarAulaAsync(moduloId, Prefixo(), concluida: true);
        await CriarAulaAsync(moduloId, Prefixo(), concluida: false);

        // Act
        var pagina = await _admin.GetFromJsonAsync<PagedResponse<AulaResponse>>($"/api/aula?moduloId={moduloId}&concluida=true");

        // Assert
        Assert.Single(pagina!.Items);
        Assert.True(pagina.Items[0].Concluida);
        Assert.Contains(pagina.Items[0].Links!, l => l.Rel == "desmarcar");
    }

    [Fact]
    public async Task PatchAula_ConcluirEDesmarcar_DeveAlternarEstado()
    {
        // Arrange
        var (_, moduloId) = await CriarTrilhaEModuloAsync();
        var aulaId = await CriarAulaAsync(moduloId, Prefixo());

        // Act
        var concluir = await _admin.PatchAsync($"/api/aula/{aulaId}/concluir", null);
        var desmarcar = await _admin.PatchAsync($"/api/aula/{aulaId}/desmarcar", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, concluir.StatusCode);
        Assert.True((await concluir.Content.ReadFromJsonAsync<AulaResponse>())!.Concluida);
        Assert.Equal(HttpStatusCode.OK, desmarcar.StatusCode);
        Assert.False((await desmarcar.Content.ReadFromJsonAsync<AulaResponse>())!.Concluida);
    }

    [Fact]
    public async Task PatchAula_ConcluirDuasVezes_DeveRetornar400BadRequest()
    {
        // Arrange
        var (_, moduloId) = await CriarTrilhaEModuloAsync();
        var aulaId = await CriarAulaAsync(moduloId, Prefixo());
        await _admin.PatchAsync($"/api/aula/{aulaId}/concluir", null);

        // Act
        var response = await _admin.PatchAsync($"/api/aula/{aulaId}/concluir", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchAula_IdInexistente_DeveRetornar404NotFound()
    {
        // Act
        var response = await _admin.PatchAsync($"/api/aula/{Guid.NewGuid()}/concluir", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EscritaTrilhaModuloAula_UsuarioPremium_DeveRetornar403Forbidden()
    {
        // Arrange
        var premium = factory.CreateAuthenticatedClient(RoleUsuario.Premium);
        var id = Guid.NewGuid();

        // Act
        var postTrilha = await premium.PostAsJsonAsync("/api/trilha", new TrilhaRequest("X", "Desc", id));
        var putModulo = await premium.PutAsJsonAsync($"/api/modulo/{id}", new ModuloUpdateRequest("X", "1 min", "Desc"));
        var postAula = await premium.PostAsJsonAsync("/api/aula", new AulaRequest("X", "Desc", 1, "Facil", false, id));
        var deleteAula = await premium.DeleteAsync($"/api/aula/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, postTrilha.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, putModulo.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, postAula.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteAula.StatusCode);
    }

    [Fact]
    public async Task LeituraTrilhaModuloAula_UsuarioPremium_DeveRetornar200Ok()
    {
        // Arrange
        var premium = factory.CreateAuthenticatedClient(RoleUsuario.Premium);

        // Act
        var trilhas = await premium.GetAsync("/api/trilha");
        var modulos = await premium.GetAsync("/api/modulo");
        var aulas = await premium.GetAsync("/api/aula");

        // Assert
        Assert.Equal(HttpStatusCode.OK, trilhas.StatusCode);
        Assert.Equal(HttpStatusCode.OK, modulos.StatusCode);
        Assert.Equal(HttpStatusCode.OK, aulas.StatusCode);
    }
}
