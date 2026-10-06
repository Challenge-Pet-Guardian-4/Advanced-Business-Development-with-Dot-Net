using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PetGuardian.Infrastructure.Mongo.Documents;

namespace PetGuardian.Infrastructure.Mongo;

/// <summary>Cria os índices da coleção em segundo plano (não bloqueia o start nem derruba a API se o Mongo cair).</summary>
public sealed class MongoIndexesInitializer(
    IMongoDatabase database,
    IOptions<MongoDbSettings> options,
    ILogger<MongoIndexesInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            var colecao = database.GetCollection<TrilhaDocument>(options.Value.TrilhasCollection);
            var keys = Builders<TrilhaDocument>.IndexKeys;

            await colecao.Indexes.CreateManyAsync(
            [
                // idempotência da sincronização (mesmo índice do script mongosh)
                new CreateIndexModel<TrilhaDocument>(keys.Ascending(d => d.TrilhaIdOrigem), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<TrilhaDocument>(keys.Ascending("pet_alvo.pet_id"))
            ], stoppingToken);

            logger.LogInformation("Índices da coleção {Colecao} garantidos.", options.Value.TrilhasCollection);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Não foi possível criar os índices do MongoDB agora; serão tentados no próximo start.");
        }
    }
}