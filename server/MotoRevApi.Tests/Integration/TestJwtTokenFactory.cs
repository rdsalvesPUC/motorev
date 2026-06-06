using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MotoRevApi.Tests.Integration;

public static class TestJwtTokenFactory
{
    private const string Secret = "PLACEHOLDER_SECRET_IN_USER_SECRETS";
    private const string Issuer = "MotoRevApi";
    private const string Audience = "MotoRevApp";

    public static string CreateToken(string role)
    {
        return CreateToken(role, Guid.NewGuid().ToString());
    }

    public static string CreateToken(string role, string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
