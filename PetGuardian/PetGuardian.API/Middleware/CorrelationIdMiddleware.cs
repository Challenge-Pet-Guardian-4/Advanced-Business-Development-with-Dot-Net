using Serilog.Context;

namespace PetGuardian.API.Middleware;

/// <summary>
/// Extrai ou gera o Correlation ID da requisição, injeta no contexto de log do Serilog
/// e adiciona o header X-Correlation-ID na resposta HTTP para rastreamento distribuído.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var correlationId = httpContext.Request.Headers.TryGetValue(CorrelationHeader, out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
            ? headerVal.ToString()
            : httpContext.TraceIdentifier;

        httpContext.Response.OnStarting(() =>
        {
            httpContext.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(httpContext);
        }
    }
}