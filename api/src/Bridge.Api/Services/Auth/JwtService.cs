using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bridge.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Bridge.Api.Services.Auth;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(JwtOptions options)
    {
        _options = options;
    }

    public string GenerateToken(User user, int? salesId, int? engineerId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        if (salesId.HasValue)
            claims.Add(new Claim("salesId", salesId.Value.ToString()));

        if (engineerId.HasValue)
            claims.Add(new Claim("engineerId", engineerId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpiryHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Bridge";
    public string Audience { get; set; } = "BridgeApi";
    public int ExpiryHours { get; set; } = 24;
}