using System.Security.Claims;
using Bridge.Api.Controllers.Auth;
using Bridge.Api.Dtos.Auth;
using Bridge.Api.Services.Auth;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Auth;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsTokenAndUserSummary_WhenCredentialsAreValid()
    {
        await using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher();
        var jwtService = CreateJwtService();
        var user = await SeedEngineerAsync(db, passwordHasher, "tanaka@bridge.local", "Password123!");
        var controller = new AuthController(db, passwordHasher, jwtService);

        var result = await controller.Login(new LoginRequest
        {
            Email = "tanaka@bridge.local",
            Password = "Password123!",
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.User.Id.Should().Be(user.Id);
        response.User.Email.Should().Be(user.Email);
        response.User.Role.Should().Be(UserRole.Engineer.ToString());
        response.User.EngineerId.Should().Be(user.Engineer!.Id);
        response.User.SalesId.Should().BeNull();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        await using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher();
        var controller = new AuthController(db, passwordHasher, CreateJwtService());
        await SeedEngineerAsync(db, passwordHasher, "tanaka@bridge.local", "Password123!");

        var result = await controller.Login(new LoginRequest
        {
            Email = "tanaka@bridge.local",
            Password = "WrongPassword",
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        await using var db = CreateDbContext();
        var controller = new AuthController(db, new PasswordHasher(), CreateJwtService());

        var result = await controller.Login(new LoginRequest
        {
            Email = "missing@bridge.local",
            Password = "Password123!",
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Me_ReturnsCurrentUser_WhenSubClaimMatchesExistingUser()
    {
        await using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher();
        var user = await SeedEngineerAsync(db, passwordHasher, "tanaka@bridge.local", "Password123!");
        var controller = new AuthController(db, passwordHasher, CreateJwtService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal(user.Id.ToString()),
            },
        };

        var result = await controller.Me();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<MeResponse>().Subject;
        response.Id.Should().Be(user.Id);
        response.Email.Should().Be("tanaka@bridge.local");
        response.Role.Should().Be(UserRole.Engineer.ToString());
        response.EngineerId.Should().Be(user.Engineer!.Id);
        response.Name.Should().Be("田中 太郎");
    }

    [Fact]
    public async Task Me_ReturnsUnauthorized_WhenSubClaimIsInvalid()
    {
        await using var db = CreateDbContext();
        var controller = new AuthController(db, new PasswordHasher(), CreateJwtService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal("not-a-number"),
            },
        };

        var result = await controller.Me();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    private static BridgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseInMemoryDatabase($"bridge-auth-tests-{Guid.NewGuid()}")
            .Options;

        return new BridgeDbContext(options);
    }

    private static JwtService CreateJwtService()
    {
        return new JwtService(new JwtOptions
        {
            Secret = "test-secret-key-for-jwt-auth-tests-32chars",
            Issuer = "Bridge.Tests",
            Audience = "Bridge.Tests",
            ExpiryHours = 1,
        });
    }

    private static ClaimsPrincipal CreatePrincipal(string sub)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", sub),
        }, "TestAuth");

        return new ClaimsPrincipal(identity);
    }

    private static async Task<User> SeedEngineerAsync(
        BridgeDbContext db,
        IPasswordHasher passwordHasher,
        string email,
        string password)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            Role = UserRole.Engineer,
        };

        var engineer = new Engineer
        {
            User = user,
            Name = "田中 太郎",
            Bio = "Web frontend engineer",
            AvoidedWorkNote = "Avoid package implementation",
        };

        db.Users.Add(user);
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();

        return user;
    }
}
