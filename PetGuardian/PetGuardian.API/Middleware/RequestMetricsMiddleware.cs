using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PetGuardian.API.Middleware;

/// <summary>
/// Expõe métricas de desempenho (tempo de resposta e taxa de erros) via
/// System.Diagnostics.Metrics, coletadas pelo OpenTelemetry (ver ObservabilityExtensions).
/// </summary>
public sealed class RequestMetricsMiddleware(RequestDelegate next)
{
    public static readonly Meter Meter = new("PetGuardian.Api", "1.0.0");

    private static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>("petguardian.http.request.duration", unit: "ms",
            description: "Tempo de resposta das requisições HTTP em milissegundos.");

    private static readonly Counter<long> RequestErrors =
        Meter.CreateCounter<long>("petguardian.http.request.errors",
            description: "Quantidade de requisições HTTP finalizadas com status >= 500.");

    private static readonly Counter<long> RequestTotal =
        Meter.CreateCounter<long>("petguardian.http.request.total",
            description: "Quantidade total de requisições HTTP recebidas.");

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(httpContext);
        }
        finally
        {
            stopwatch.Stop();

            var route = httpContext.GetEndpoint()?.DisplayName ?? httpContext.Request.Path.Value ?? "unknown";
            var tags = new TagList
            {
                { "route", route },
                { "method", httpContext.Request.Method },
                { "status_code", httpContext.Response.StatusCode }
            };

            RequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
            RequestTotal.Add(1, tags);

            if (httpContext.Response.StatusCode >= 500)
                RequestErrors.Add(1, tags);
        }
    }
}