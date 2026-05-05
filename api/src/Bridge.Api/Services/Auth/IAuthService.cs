using Bridge.Api.Dtos.Auth;

namespace Bridge.Api.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<MeResponse?> GetMeAsync(int userId);
}
