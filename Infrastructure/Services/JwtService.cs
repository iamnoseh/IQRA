using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    private readonly string _key = configuration["Jwt:Key"] ??
                                   throw new InvalidOperationException("Jwt:Key not configured");
    private readonly string _issuer = configuration["Jwt:Issuer"] ??
                                      throw new InvalidOperationException("Jwt:Issuer not configured");
    private readonly string _audience = configuration["Jwt:Audience"] ??
                                        throw new InvalidOperationException("Jwt:Audience not configured");
    private readonly int _expiresMinutes = int.Parse(configuration["Jwt:ExpiresMinutes"] ?? "60");

    public string GenerateToken(AppUser user)
    {
        var claims = BuildClaims(user);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            tokenHandler.ValidateToken(token, BuildValidationParameters(key), out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(x => x.Type == "UserId").Value;

            return Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }

    private static List<Claim> BuildClaims(AppUser user) => new()
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim("UserId", user.Id.ToString())
    };

    private TokenValidationParameters BuildValidationParameters(byte[] key) => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = _issuer,
        ValidateAudience = true,
        ValidAudience = _audience,
        ClockSkew = TimeSpan.Zero
    };
}
