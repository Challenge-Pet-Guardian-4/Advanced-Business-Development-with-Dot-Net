using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetGuardian.API.Health;
using PetGuardian.API.HealthChecks;
using PetGuardian.API.Middleware;
using PetGuardian.Application.Common;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.API.Extensions;

/// <summary>
/// Concentra a configuração de Monitoramento e Observabilidade pedida pelo enunciado:
/// Health Checks, Distributed Tracing e Métricas via OpenTelemetry.
/// (O logging estruturado com Serilog é configurado separadamente em Program.cs, antes do WebApplication.CreateBuilder,
/// pois precisa ser plugado no host builder.)
/// </summary>
public static class ObservabilityExtensions
{
    private const string ServiceName = "PetGuardian.API";

    public static IServiceCollection AddPetGuardianObservability(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ----- Health Checks (Microsoft.Extensions.Diagnostics.HealthChecks, já vem no SDK Web) -----
        services.AddHttpClient("ViaCepHealthCheck", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Processo da API ativo e operacional."), tags: ["live"])
            .AddCheck<OracleDbHealthCheck>(
                "oracle-database",
                tags: ["ready", "db"])
            .AddCheck<ViaCepHealthCheck>(
                "external-service-viacep",
                tags: ["ready", "external"]);

        // ----- OpenTelemetry: Tracing + Métricas -----
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .AddSource(PetGuardianActivitySource.SourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(RequestMetricsMiddleware.Meter.Name)
                .AddMeter(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Routing",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "System.Net.Http",
                    "System.Runtime")
                .AddConsoleExporter()
                .AddPrometheusExporter());

        return services;
    }

    public static WebApplication UsePetGuardianObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestMetricsMiddleware>();

        // Endpoint oficial de scraping do Prometheus (/metrics)
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        // /health -> visão geral (usada por orquestradores simples)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

        // /health/ready -> só os checks marcados como "ready" (banco + serviços externos)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

        // /health/live -> liveness simples, sem dependências externas (check do processo)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

        return app;
    }
}