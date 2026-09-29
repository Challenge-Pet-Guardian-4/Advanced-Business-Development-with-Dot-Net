using System.Net;
using System.Text.Json;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class HealthCheckEndpointsTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_EndpointGeral_DeveRetornarStatus200EJsonValido()
    {
        // Arrange
        // (Sem payload adicional necessário para endpoint GET de monitoramento)

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("status", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthReady_EndpointProntidao_DeveRetornarStatus200()
    {
        // Arrange
        // (Sem payload adicional necessário para probe de prontidão)

        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("status", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthLive_EndpointLiveness_DeveRetornarStatus200()
    {
        // Arrange
        // (Sem payload adicional necessário para probe de liveness)

        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_EndpointPrometheus_DeveRetornarStatus200ETextoPrometheus()
    {
        // Arrange
        // (Scraping endpoint do Prometheus configurado em /metrics)

        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }
}
