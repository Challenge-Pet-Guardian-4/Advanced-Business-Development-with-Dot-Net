using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PetGuardian.API.Health;

/// <summary>
/// Serializador de respostas JSON customizado para os endpoints de Health Check (/health, /health/ready, /health/live).
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                error = e.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
