using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bridge.Api.Services.Auth;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using FluentAssertions;

namespace Bridge.Tests.Auth;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_IncludesUserRoleAndProfileClaims()
    {
        var service = new JwtService(new JwtOptions
        {
            Secret = "test-secret-key-for-jwt-service-tests-32chars",
            Issuer = "Bridge.Tests",
            Audience = "Bridge.Tests",
            ExpiryHours = 2,
        });
        var user = new User
        {
            Id = 10,
            Email = "sato@bridge.local",
            Role = UserRole.Sales,
        };

        var token = service.GenerateToken(user, salesId: 20, engineerId: null);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("Bridge.Tests");
        jwt.Audiences.Should().Contain("Bridge.Tests");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "10");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "sato@bridge.local");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == UserRole.Sales.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "salesId" && c.Value == "20");
        jwt.Claims.Should().NotContain(c => c.Type == "engineerId");
    }
}
