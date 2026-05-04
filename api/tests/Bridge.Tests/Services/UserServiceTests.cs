using Bridge.Api.Dtos.Users;
using Bridge.Api.Services.Auth;
using Bridge.Api.Services.Users;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesSalesUserAndProfile()
    {
        await using var db = TestDbContextFactory.Create("bridge-user-service-tests");
        var service = new UserService(db, new TestPasswordHasher());

        var result = await service.CreateAsync(new CreateUserRequest
        {
            Email = "sato@bridge.local",
            Password = "Password123!",
            Role = "Sales",
            Name = "佐藤 営業",
            Department = "Sales",
        });

        result.Success.Should().BeTrue();
        result.Response!.Email.Should().Be("sato@bridge.local");
        result.Response.Role.Should().Be(UserRole.Sales.ToString());
        result.Response.SalesId.Should().NotBeNull();
        result.Response.EngineerId.Should().BeNull();

        var user = await db.Users.Include(u => u.Sales).SingleAsync();
        user.PasswordHash.Should().Be("hashed:Password123!");
        user.Sales!.Name.Should().Be("佐藤 営業");
        user.Sales.Department.Should().Be("Sales");
    }

    [Fact]
    public async Task CreateAsync_CreatesEngineerUserAndProfile()
    {
        await using var db = TestDbContextFactory.Create("bridge-user-service-tests");
        var sales = new Sales { Name = "佐藤 営業" };
        db.Sales.Add(sales);
        await db.SaveChangesAsync();
        var service = new UserService(db, new TestPasswordHasher());

        var result = await service.CreateAsync(new CreateUserRequest
        {
            Email = "tanaka@bridge.local",
            Password = "Password123!",
            Role = "Engineer",
            Name = "田中 太郎",
            PrimarySalesId = sales.Id,
        });

        result.Success.Should().BeTrue();
        result.Response!.EngineerId.Should().NotBeNull();
        result.Response.SalesId.Should().BeNull();

        var engineer = await db.Engineers.Include(e => e.User).SingleAsync();
        engineer.Name.Should().Be("田中 太郎");
        engineer.PrimarySalesId.Should().Be(sales.Id);
        engineer.User.Email.Should().Be("tanaka@bridge.local");
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicateError_WhenEmailAlreadyExists()
    {
        await using var db = TestDbContextFactory.Create("bridge-user-service-tests");
        db.Users.Add(new User
        {
            Email = "existing@bridge.local",
            PasswordHash = "hash",
            Role = UserRole.Admin,
        });
        await db.SaveChangesAsync();
        var service = new UserService(db, new TestPasswordHasher());

        var result = await service.CreateAsync(new CreateUserRequest
        {
            Email = "existing@bridge.local",
            Password = "Password123!",
            Role = "Sales",
            Name = "佐藤 営業",
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_ALREADY_EXISTS");
        (await db.Users.CountAsync()).Should().Be(1);
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string hashedPassword) => hashedPassword == Hash(password);
    }
}
