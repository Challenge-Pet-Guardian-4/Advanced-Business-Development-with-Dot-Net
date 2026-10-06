using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGuardian.API;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;
using PetGuardian.Infrastructure.Mongo;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSecret = "TesteIntegracao-PetGuardian-ChaveJwt-Com-Mais-De-32-Caracteres!";

    private readonly string _databaseName = "PetGuardian_IntegrationTestDb_" + Guid.NewGuid();
    private readonly ConcurrentDictionary<RoleUsuario, string> _tokens = new();
    private readonly TokenService _tokenService = new(
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SecretKey"] = TestJwtSecret })
            .Build());

    /// <summary>Cliente com token JWT de um usuário de teste com o perfil informado (Admin por padrão).</summary>
    public HttpClient CreateAuthenticatedClient(RoleUsuario role = RoleUsuario.Admin)
    {
        var token = _tokens.GetOrAdd(role, r => _tokenService.GerarToken(
            new Usuario($"Teste {r}", $"teste.{r}@petguardian.com".ToLowerInvariant(), "SenhaTeste@123", r, Guid.NewGuid())));
        return CreateClientWithToken(token);
    }

    /// <summary>Cliente autenticado como um usuário específico (para testar regras "somente o próprio usuário").</summary>
    public HttpClient CreateClientFor(Usuario usuario) => CreateClientWithToken(_tokenService.GerarToken(usuario));

    private HttpClient CreateClientWithToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:SecretKey", TestJwtSecret);
        builder.UseSetting("MongoDb:ConnectionString", "mongodb://localhost:27017");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PetGuardianOracle"] = "User Id=RM000000;Password=dummy;Data Source=localhost:1521/orcl;",
                ["Jwt:SecretKey"] = TestJwtSecret,
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove todas as configurações do DbContext e do provedor Oracle
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<PetGuardianContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(PetGuardianContext) ||
                (d.ServiceType.FullName != null && (
                    d.ServiceType.FullName.Contains("PetGuardianContext") ||
                    d.ServiceType.FullName.Contains("Oracle")
                ))
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<PetGuardianContext>(options => options.UseInMemoryDatabase(_databaseName));

            // ---- MongoDB: sem servidor real nos testes ----
            services.RemoveAll<IMongoDatabase>();
            services.RemoveAll<ITrilhaCatalogoRepository>();
            foreach (var hosted in services.Where(d =>
                         d.ServiceType == typeof(IHostedService) &&
                         d.ImplementationType == typeof(MongoIndexesInitializer)).ToList())
                services.Remove(hosted);

            // Health check do Mongo: ping simulado
            var mongoDb = new Mock<IMongoDatabase>();
            mongoDb.Setup(d => d.RunCommandAsync(
                    It.IsAny<Command<BsonDocument>>(), It.IsAny<ReadPreference>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BsonDocument("ok", 1));
            services.AddSingleton(mongoDb.Object);

            // Catálogo: implementação em memória (mesmo contrato do repositório Mongo)
            services.AddSingleton<ITrilhaCatalogoRepository, InMemoryTrilhaCatalogoRepository>();

            // Substitui IViaCepService por mock determinístico
            var viaCepDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IViaCepService));
            if (viaCepDescriptor != null)
                services.Remove(viaCepDescriptor);

            var mockViaCep = new Mock<IViaCepService>();
            mockViaCep.Setup(v => v.ConsultarCepAsync(It.Is<string>(c => c.Contains("01001000") || c.Contains("01001-000")), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ViaCepResponseDto(
                    Cep: "01001-000",
                    Logradouro: "Praça da Sé",
                    Complemento: "lado ímpar",
                    Bairro: "Sé",
                    Localidade: "São Paulo",
                    Uf: "SP",
                    Estado: "São Paulo",
                    Erro: null
                ));

            mockViaCep.Setup(v => v.ConsultarCepAsync(It.Is<string>(c => c.Contains("99999999")), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ViaCepResponseDto?)null);

            services.AddSingleton(mockViaCep.Object);

            // Popula dados essenciais
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PetGuardianContext>();
            context.Database.EnsureCreated();
            SeedTestData(context);
        });
    }

    private static void SeedTestData(PetGuardianContext context)
    {
        if (!context.Status.Any())
        {
            context.Status.AddRange(
                new Status("PENDENTE"),
                new Status("CONCLUIDO"),
                new Status("EXPIRADO")
            );
        }

        if (!context.Racas.Any())
        {
            context.Racas.AddRange(
                new Raca("Labrador"),
                new Raca("Poodle"),
                new Raca("Siamês")
            );
        }

        if (!context.Telefones.Any())
            context.Telefones.Add(new Telefone("11", "999998888"));

        context.SaveChanges();
    }
}