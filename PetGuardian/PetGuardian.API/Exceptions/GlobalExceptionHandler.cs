using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using PetGuardian.API.Middleware;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.API.Exceptions;

/// <summary>
/// Intercepta todas as exceções não tratadas e devolve um <c>ProblemDetails</c> padronizado (RFC 7807)
/// com rastreamento distribuído (Trace ID e Correlation ID).
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(exception, "Exceção capturada no pipeline: {Message} : TraceId {TraceId}", exception.Message, traceId);

        var (statusCode, title, detail) = MapException(exception, environment);

        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type     = "about:blank",
            Title    = title,
            Status   = statusCode,
            Detail   = detail,
            Instance = httpContext.Request.Path
        };

        // Rastreabilidade padronizada para diagnóstico (W3C OpenTelemetry / Kestrel)
        problem.Extensions["traceId"] = traceId;

        if (httpContext.Request.Headers.TryGetValue(CorrelationIdMiddleware.CorrelationHeader, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            problem.Extensions["correlationId"] = correlationId.ToString();
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(
        Exception exception,
        IHostEnvironment environment) => exception switch
    {
        ArgumentNullException e       => (StatusCodes.Status400BadRequest,  "Requisição inválida",                  e.Message),
        ArgumentException e           => (StatusCodes.Status400BadRequest,  "Requisição inválida",                  e.Message),
        DomainException e             => (StatusCodes.Status400BadRequest,  "Regra de negócio violada",             e.Message),
        InvalidOperationException e   => (StatusCodes.Status400BadRequest,  "Não foi possível concluir a operação", e.Message),
        KeyNotFoundException e        => (StatusCodes.Status404NotFound,    "Recurso não encontrado",               e.Message),
        UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized,"Não autorizado",                       e.Message),
        DbUpdateException e           => (StatusCodes.Status409Conflict,    "Conflito de dados no banco de dados",  e.InnerException?.Message ?? e.Message),
        OracleException e             => (StatusCodes.Status502BadGateway,  "Banco de dados indisponível",          e.Message),
        _                             => MapUnhandled(environment, exception)
    };

    private static (int StatusCode, string Title, string? Detail) MapUnhandled(
        IHostEnvironment environment,
        Exception exception) =>
    (
        StatusCodes.Status500InternalServerError,
        "Erro interno do servidor",
        environment.IsDevelopment()
            ? exception.ToString()
            : "Ocorreu um erro inesperado. Tente novamente mais tarde."
    );
}
