using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PetGuardian.Infrastructure.Mongo.Documents;

namespace PetGuardian.Infrastructure.Mongo;

/// <summary>Cria os índices da coleção de conteúdos de aula em segundo plano no MongoDB Atlas.</summary>
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
            await GarantirSchemaValidatorAsync(stoppingToken);
            await CriarIndicesConteudosAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Não foi possível configurar a coleção do MongoDB Atlas no momento. Uma nova tentativa ocorrerá na próxima inicialização.");
        }
    }

    private async Task GarantirSchemaValidatorAsync(CancellationToken stoppingToken)
    {
        var collectionName = options.Value.ConteudosAulaCollection;
        var command = new BsonDocument
        {
            { "collMod", collectionName },
            {
                "validator", new BsonDocument("$jsonSchema", new BsonDocument
                {
                    { "bsonType", "object" },
                    { "required", new BsonArray { "aula_id", "tipo_conteudo", "corpo_markdown" } },
                    {
                        "properties", new BsonDocument
                        {
                            { "aula_id", new BsonDocument { { "bsonType", new BsonArray { "int", "long", "string" } } } },
                            { "tipo_conteudo", new BsonDocument { { "bsonType", "string" } } },
                            { "corpo_markdown", new BsonDocument { { "bsonType", "string" } } },
                            { "links_recursos", new BsonDocument { { "bsonType", "array" }, { "items", new BsonDocument("bsonType", "string") } } },
                            { "atualizado_em", new BsonDocument { { "bsonType", new BsonArray { "string", "date" } } } }
                        }
                    }
                })
            }
        };

        try
        {
            await database.RunCommandAsync<BsonDocument>(command, cancellationToken: stoppingToken);
        }
        catch (MongoCommandException)
        {
            // Ignora se a coleção ainda não existir (será criada na primeira escrita ou criação de índice)
        }
    }

    private async Task CriarIndicesConteudosAsync(CancellationToken stoppingToken)
    {
        var colecao = database.GetCollection<ConteudoAulaDocument>(options.Value.ConteudosAulaCollection);
        var keys = Builders<ConteudoAulaDocument>.IndexKeys;

        CreateIndexModel<ConteudoAulaDocument>[] models =
        [
            new(keys.Ascending(d => d.AulaId), new CreateIndexOptions { Unique = true, Name = "idx_aula_id_unique" }),
            new(keys.Text(d => d.CorpoMarkdown), new CreateIndexOptions { Name = "idx_busca_textual_conteudo" }),
            new(keys.Ascending(d => d.TipoConteudo), new CreateIndexOptions { Name = "idx_tipo_conteudo" })
        ];

        foreach (var model in models)
        {
            try
            {
                await colecao.Indexes.CreateOneAsync(model, cancellationToken: stoppingToken);
            }
            catch (MongoCommandException ex) when (ex.Code is 85 or 86)
            {
                // Índice equivalente já existe no cluster (ex.: criado pelo serviço Java com nome "aula_id")
            }
        }

        logger.LogInformation("Índices da coleção {Colecao} no MongoDB Atlas garantidos com sucesso.", options.Value.ConteudosAulaCollection);
    }
}