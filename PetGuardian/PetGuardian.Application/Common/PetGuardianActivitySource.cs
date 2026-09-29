using System.Diagnostics;

namespace PetGuardian.Application.Common;

/// <summary>
/// Provedor centralizado de ActivitySource para Distributed Tracing entre camadas (OpenTelemetry).
/// Permite rastrear requisições desde a camada de API até a execução nos Services da Application.
/// </summary>
public static class PetGuardianActivitySource
{
    public const string SourceName = "PetGuardian.Application";
    public const string Version = "1.0.0";

    public static readonly ActivitySource Source = new(SourceName, Version);
}
