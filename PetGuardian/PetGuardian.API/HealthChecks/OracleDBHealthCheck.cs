using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.API.HealthChecks;

/// <summary>
/// Verifica conectividade com o banco (Oracle) usando o próprio EF Core
/// (Database.CanConnectAsync), sem depender de nenhum pacote extra de Health Check para Oracle.
/// </summary>
public sealed class OracleDbHealthCheck(PetGuardianContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var podeConectar = await context.Database.CanConnectAsync(cancellationToken);
            return podeConectar
                ? HealthCheckResult.Healthy("Conexão com o Oracle estabelecida com sucesso.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao verificar conectividade com o Oracle.", ex);
        }
    }
}