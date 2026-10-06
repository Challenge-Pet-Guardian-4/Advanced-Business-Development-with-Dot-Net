using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PetGuardian.API.HealthChecks;

/// <summary>Verifica a conectividade com o MongoDB via comando "ping".</summary>
public sealed class MongoDbHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Conexão com o MongoDB estabelecida com sucesso.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao verificar conectividade com o MongoDB.", ex);
        }
    }
}