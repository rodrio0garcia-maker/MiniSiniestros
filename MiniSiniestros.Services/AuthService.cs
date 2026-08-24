using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MiniSiniestros.Services;

public class AuthService : IAuthService
{
    // Usuario hardcodeado a propósito para esta demo.
    // En producción sería una tabla Users con password hasheado (BCrypt/Identity).
    private const string HardcodedUsername = "operador";
    private const string HardcodedPassword = "Operador123!";
    private const string HardcodedRole = "Operador";

    private readonly JwtSettingsBase _jwtSettings;

    public AuthService(JwtSettingsBase jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string? ValidarYGenerarToken(string username, string password)
    {
        if (username != HardcodedUsername || password != HardcodedPassword)
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, HardcodedRole)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}