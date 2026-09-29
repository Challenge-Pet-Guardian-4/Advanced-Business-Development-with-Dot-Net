using System.Net;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;

namespace PetGuardian.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ObservabilityMiddlewareTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Request_SemHeaderCorrelationId_DeveGerarERetornarCorrelationIdNoHeader()
    {
        // Arrange
        // (Sem header prévio no request)

        // Act
        var response = await _client.GetAsync("/api/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task Request_ComHeaderCorrelationId_DevePropagarMesmoCorrelationIdNoHeader()
    {
        // Arrange
        var customId = "teste-correlation-" + Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/status");
        request.Headers.Add("X-Correlation-ID", customId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(customId, correlationId);
    }
}
