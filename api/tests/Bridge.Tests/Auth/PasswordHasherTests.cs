using Bridge.Api.Services.Auth;
using FluentAssertions;

namespace Bridge.Tests.Auth;

public class PasswordHasherTests
{
    [Fact]
    public void Verify_ReturnsTrueForMatchingPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        hasher.Verify("Password123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalseForDifferentPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        hasher.Verify("WrongPassword", hash).Should().BeFalse();
    }
}
