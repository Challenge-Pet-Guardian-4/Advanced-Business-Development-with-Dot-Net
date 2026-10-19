using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PetGuardian.Application.Repositories;
using PetGuardian.Infrastructure.Mongo;

namespace PetGuardian.API.Extensions;

public static class MongoExtensions
{
    /// <summary>Registra o cliente do MongoDB Atlas e o repositório de conteúdos de aula.</summary>
    public static IServiceCollection AddPetGuardianMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MongoDbSettings>().Bind(configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var s = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var connStr = string.IsNullOrWhiteSpace(s.ConnectionString)
                ? configuration["MONGODB_URI"]
                : s.ConnectionString;

            if (string.IsNullOrWhiteSpace(connStr))
                connStr = "mongodb://localhost:27017";

            var settings = MongoClientSettings.FromConnectionString(connStr);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(s.ServerSelectionTimeoutSeconds);
            return new MongoClient(settings);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var s = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return client.GetDatabase(configuration["MONGODB_DATABASE"] ?? s.DatabaseName);
        });

        services.AddSingleton<IConteudoAulaRepository, ConteudoAulaRepository>();
        services.AddHostedService<MongoIndexesInitializer>();

        return services;
    }
}