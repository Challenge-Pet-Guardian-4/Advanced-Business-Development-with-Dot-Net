using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Implementação do serviço de token JWT utilizando a biblioteca oficial da Microsoft (System.IdentityModel.Tokens.Jwt).
/// Adere aos padrões SOLID (SRP, ISP, DIP), Clean Code e DRY.
/// </summary>
public class TokenService(IConfiguration configuration) : ITokenService
{
    public const string DefaultSecret = "PetGuardianChallenge2026SuperSecretKeySecurityJWT100%!";
    public const string Issuer = "PetGuardian.API";
    public const string Audience = "PetGuardian.Clients";
    public const int ExpirationHours = 2;

    private byte[] GetKeyBytes()
    {
        var secret = configuration["Jwt:SecretKey"] ?? DefaultSecret;
        return Encoding.UTF8.GetBytes(secret.PadRight(32));
    }

    public string GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(GetKeyBytes());
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

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public (bool IsValido, ClaimsPrincipal? Principal) ValidarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(GetKeyBytes());

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return (true, principal);
        }
        catch
        {
            return (false, null);
        }
    }
}
