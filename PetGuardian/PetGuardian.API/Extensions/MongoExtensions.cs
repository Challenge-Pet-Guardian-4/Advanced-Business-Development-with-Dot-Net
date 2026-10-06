using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PetGuardian.Application.Repositories;
using PetGuardian.Infrastructure.Mongo;

namespace PetGuardian.API.Extensions;

public static class MongoExtensions
{
    /// <summary>Registra MongoDB (cliente, database, repositório do catálogo e criação de índices).</summary>
    public static IServiceCollection AddPetGuardianMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MongoDbSettings>().Bind(configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var s = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            if (string.IsNullOrWhiteSpace(s.ConnectionString))
                throw new InvalidOperationException(
                    "MongoDb:ConnectionString não configurada. Defina via variável de ambiente MongoDb__ConnectionString.");

            var settings = MongoClientSettings.FromConnectionString(s.ConnectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(s.ServerSelectionTimeoutSeconds);
            return new MongoClient(settings);
        });

        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>()
            .GetDatabase(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.DatabaseName));

        services.AddSingleton<ITrilhaCatalogoRepository, TrilhaCatalogoRepository>();
        services.AddHostedService<MongoIndexesInitializer>();

        return services;
    }
}