using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PetGuardian.API.HealthChecks;

/// <summary>
/// Verifica a disponibilidade do serviço externo consumido por EnderecoService (ViaCEP).
/// Usa um HttpClient nomeado ("ViaCepHealthCheck") registrado em Program.cs com timeout curto.
/// </summary>
public sealed class ViaCepHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ViaCepHealthCheck");
            // CEP fixo e válido, só para medir se o serviço responde.
            using var response = await client.GetAsync("https://viacep.com.br/ws/01001000/json/", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Serviço ViaCEP respondendo normalmente.")
                : HealthCheckResult.Degraded($"ViaCEP retornou status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            // Degraded (não Unhealthy): a API continua funcional sem o ViaCEP, apenas o cadastro de
            // endereço por CEP fica indisponível.
            return HealthCheckResult.Degraded("Serviço ViaCEP indisponível no momento.", ex);
        }
    }
}