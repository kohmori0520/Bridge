using Bridge.Domain.Entities;

namespace Bridge.Api.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user, int? salesId, int? engineerId);
}