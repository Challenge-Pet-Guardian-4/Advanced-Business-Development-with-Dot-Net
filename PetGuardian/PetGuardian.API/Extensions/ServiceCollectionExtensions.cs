using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Infrastructure.Persistence;
using PetGuardian.Infrastructure.Persistence.Repositories;

namespace PetGuardian.API.Extensions;

/// <summary>
/// Extensões para registrar persistência, repositórios e serviços da solução PetGuardian na injeção de dependências.
/// </summary>
public static class PetGuardianServiceCollectionExtensions
{
    /// <summary>Registra o <see cref="PetGuardianContext"/> com Oracle.</summary>
    public static IServiceCollection AddPetGuardianDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "PetGuardianOracle")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' não encontrada.");

        services.AddDbContext<PetGuardianContext>(options =>
            options.UseOracle(connectionString, b =>
                b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

        return services;
    }

    /// <summary>Registra todas as implementações de repositório como <c>Scoped</c> (um por requisição HTTP).</summary>
    public static IServiceCollection AddPetGuardianRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<ITrilhaRepository, TrilhaRepository>();
        services.AddScoped<IModuloRepository, ModuloRepository>();
        services.AddScoped<IAulaRepository, AulaRepository>();
        services.AddScoped<IHistoricoRepository, HistoricoRepository>();
        services.AddScoped<IUsuarioPetRepository, UsuarioPetRepository>();
        services.AddScoped<IUsuarioEnderecoRepository, UsuarioEnderecoRepository>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>Adiciona serviços que orquestram regras de negócio e integrações externas.</summary>
    public static IServiceCollection AddPetGuardianApplicationServices(this IServiceCollection services)
    {
        // Integração externa
        services.AddHttpClient<IViaCepService, ViaCepService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        // Hierarquia de endereço
        services.AddScoped<IEstadoService,   EstadoService>();
        services.AddScoped<ICidadeService,   CidadeService>();
        services.AddScoped<IBairroService,   BairroService>();
        services.AddScoped<IEnderecoService, EnderecoService>();

        // Lookup
        services.AddScoped<IRacaService,     RacaService>();
        services.AddScoped<IStatusService,   StatusService>();
        services.AddScoped<ITelefoneService, TelefoneService>();

        // Core
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<ITarefaService, TarefaService>();
        services.AddScoped<ITrilhaService, TrilhaService>();
        services.AddScoped<IModuloService, ModuloService>();
        services.AddScoped<IAulaService, AulaService>();
        services.AddScoped<IHistoricoService, HistoricoService>();

        // Join tables
        services.AddScoped<IUsuarioPetService, UsuarioPetService>();
        services.AddScoped<IUsuarioEnderecoService, UsuarioEnderecoService>();

        // Segurança / Autenticação
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    /// <summary>Configura autenticação JWT Bearer oficial da Microsoft e autorização.</summary>
    public static IServiceCollection AddPetGuardianJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:SecretKey"] ?? TokenService.DefaultSecret;
        var keyBytes = Encoding.UTF8.GetBytes(jwtSecret.PadRight(32));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = TokenService.Issuer,
                ValidateAudience = true,
                ValidAudience = TokenService.Audience,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
