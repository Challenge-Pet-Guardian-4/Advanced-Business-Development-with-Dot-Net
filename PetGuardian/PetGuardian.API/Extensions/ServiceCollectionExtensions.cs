using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetGuardian.API.Security;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Implementations;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Enums;
using PetGuardian.Infrastructure.Persistence;
using PetGuardian.Infrastructure.Persistence.Repositories;

namespace PetGuardian.API.Extensions;

/// <summary>
/// Extensões para registrar persistência, repositórios, serviços e segurança da solução PetGuardian.
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

    /// <summary>Registra as implementações de repositório relacionais como <c>Scoped</c>.</summary>
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

        // Consultas paginadas (leitura)
        services.AddScoped<IPetQueryService, PetQueryService>();
        services.AddScoped<ITarefaQueryService, TarefaQueryService>();
        services.AddScoped<IUsuarioQueryService, UsuarioQueryService>();
        services.AddScoped<IHistoricoQueryService, HistoricoQueryService>();

        // Catálogo NoSQL (MongoDB)
        services.AddScoped<ITrilhaCatalogoService, TrilhaCatalogoService>();

        // Join tables
        services.AddScoped<IUsuarioPetService, UsuarioPetService>();
        services.AddScoped<IUsuarioEnderecoService, UsuarioEnderecoService>();

        // Segurança / Autenticação
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// JWT Bearer + autorização: toda rota exige usuário autenticado (fallback policy),
    /// exceto as marcadas com [AllowAnonymous]; policy "AdminOnly" para operações administrativas.
    /// O secret é lido de forma lazy da configuração final (variável de ambiente / user-secrets).
    /// </summary>
    public static IServiceCollection AddPetGuardianJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
        });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, cfg) =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(TokenService.ResolveKeyBytes(cfg)),
                    ValidateIssuer = true,
                    ValidIssuer = TokenService.Issuer,
                    ValidateAudience = true,
                    ValidAudience = TokenService.Audience,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(nameof(RoleUsuario.Admin)));

        return services;
    }
}