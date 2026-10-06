using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>Emissão/validação de JWT (RFC 7519, HS256). O secret vem SEMPRE de configuração (Jwt:SecretKey).</summary>
public class TokenService(IConfiguration configuration) : ITokenService
{
    public const string Issuer = "PetGuardian.API";
    public const string Audience = "PetGuardian.Clients";
    public const int ExpirationHours = 2;
    public const int MinimumSecretLength = 32;

    /// <summary>Lê e valida o secret. Falha explicitamente se ausente/curto (nada de fallback hardcoded).</summary>
    public static byte[] ResolveKeyBytes(IConfiguration configuration)
    {
        var secret = configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < MinimumSecretLength)
            throw new InvalidOperationException(
                $"Jwt:SecretKey ausente ou com menos de {MinimumSecretLength} caracteres. " +
                "Defina via variável de ambiente Jwt__SecretKey ou dotnet user-secrets.");

        return Encoding.UTF8.GetBytes(secret);
    }

    public string GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(ResolveKeyBytes(configuration));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(ExpirationHours),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = credentials
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    public (bool IsValido, ClaimsPrincipal? Principal) ValidarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(ResolveKeyBytes(configuration)),
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return (true, principal);
        }
        catch
        {
            return (false, null);
        }
    }
}
