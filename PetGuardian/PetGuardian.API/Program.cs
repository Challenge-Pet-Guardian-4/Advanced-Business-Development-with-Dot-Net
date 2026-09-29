using System.Reflection;
using Microsoft.OpenApi;
using PetGuardian.API.Exceptions;
using PetGuardian.API.Extensions;
using Serilog;

namespace PetGuardian.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ----- SPRINT 3: Logging estruturado com Serilog -----
        // Console + arquivo (rolling diário), níveis padrão de Information/Warning/Error,
        // enriquecido com CorrelationId via CorrelationIdMiddleware (ver Extensions/ObservabilityExtensions.cs).
        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PetGuardian.API")
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/petguardian-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {SourceContext}: {Message:lj}{NewLine}{Exception}"));

        builder.Services.AddPetGuardianDbContext(builder.Configuration);
        builder.Services.AddPetGuardianRepositories();
        builder.Services.AddPetGuardianApplicationServices();
        builder.Services.AddPetGuardianJwtAuthentication(builder.Configuration);
        builder.Services.AddPetGuardianObservability(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PetGuardian API",
                Version = "v1",
                Description = "API REST para gerenciamento da rede de cuidado colaborativo de pets.",
                Contact = new OpenApiContact
                {
                    Name = "Equipe PetGuardian",
                    Email = "contato@petguardian.com"
                }
            });
            var apiXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXml);
            if (File.Exists(apiXmlPath))
            {
                options.IncludeXmlComments(apiXmlPath);
            }
            var appXml = "PetGuardian.Application.xml";
            var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXml);
            if (File.Exists(appXmlPath))
            {
                options.IncludeXmlComments(appXmlPath);
            }
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato: Bearer {token}"
            });
        });

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        // Observabilidade (CorrelationId, Prometheus /metrics e Probes /health) antes do SwaggerUI
        app.UsePetGuardianObservability();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PetGuardian API v1");
            options.RoutePrefix = string.Empty;
        });
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
}