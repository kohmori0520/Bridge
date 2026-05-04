using Microsoft.AspNetCore.Identity;
using Bridge.Domain.Entities;

namespace Bridge.Api.Services.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
    {
        // User インスタンスは PasswordHasher の API 都合で必要(中身は使われない)
        return _hasher.HashPassword(new User(), password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        var result = _hasher.VerifyHashedPassword(new User(), hashedPassword, password);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}