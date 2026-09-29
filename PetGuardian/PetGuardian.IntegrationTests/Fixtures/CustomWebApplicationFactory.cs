using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetGuardian.API;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "PetGuardian_IntegrationTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PetGuardianOracle"] = "User Id=RM000000;Password=dummy;Data Source=localhost:1521/orcl;"
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
            {
                services.Remove(descriptor);
            }

            // Adiciona DbContext isolado usando InMemory Database
            services.AddDbContext<PetGuardianContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Substitui IViaCepService por mock determinístico
            var viaCepDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IViaCepService));

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
        {
            context.Telefones.Add(new Telefone("11", "999998888"));
        }

        context.SaveChanges();
    }
}
