using Bridge.Api.Dtos.Users;

namespace Bridge.Api.Services.Users;

public interface IUserService
{
    Task<CreateUserResult> CreateAsync(CreateUserRequest request);
}

public record CreateUserResult(
    bool Success,
    CreateUserResponse? Response,
    string? ErrorCode,
    string? ErrorMessage);