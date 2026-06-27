using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PandaKey.Api.Models;

namespace PandaKey.Api.Services;

/// <summary>
/// Issues signed JWTs for authenticated users. The token carries the user id
/// (sub / NameIdentifier), e-mail, display name and the role claim that
/// [Authorize(Roles="admin")] checks on the admin endpoints.
/// </summary>
public sealed class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public string CreateToken(UserDto user)
    {
        var section = _config.GetSection("Jwt");
        var key = section["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = section["Issuer"] ?? "PandaKey";
        var audience = section["Audience"] ?? "PandaKeyClients";
        var expiryMinutes = int.TryParse(section["ExpiryMinutes"], out var m) ? m : 720;

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
