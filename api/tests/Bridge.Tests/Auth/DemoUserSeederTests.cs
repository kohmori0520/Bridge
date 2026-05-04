using Bridge.Api.Services.Auth;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Auth;

public class DemoUserSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesDemoUsers_WhenDatabaseIsEmpty()
    {
        await using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher();

        await DemoUserSeeder.SeedAsync(db, passwordHasher);

        var users = await db.Users
            .Include(u => u.Sales)
            .Include(u => u.Engineer)
            .ToListAsync();

        users.Should().HaveCount(3);
        users.Should().Contain(u => u.Email == "admin@bridge.local" && u.Role == UserRole.Admin);
        users.Should().Contain(u => u.Email == "sato@bridge.local" && u.Sales != null);

        var engineer = await db.Engineers.Include(e => e.PrimarySales).SingleAsync();
        engineer.User.Email.Should().Be("tanaka@bridge.local");
        engineer.PrimarySalesId.Should().NotBeNull();
        engineer.PrimarySales!.Name.Should().Be("佐藤 営業");
        passwordHasher.Verify("Engineer1234!", engineer.User.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_DoesNothing_WhenAnyUserAlreadyExists()
    {
        await using var db = CreateDbContext();
        db.Users.Add(new()
        {
            Email = "existing@bridge.local",
            PasswordHash = "hash",
            Role = UserRole.Admin,
        });
        await db.SaveChangesAsync();

        await DemoUserSeeder.SeedAsync(db, new PasswordHasher());

        var users = await db.Users.ToListAsync();
        users.Should().ContainSingle();
        users[0].Email.Should().Be("existing@bridge.local");
    }

    private static BridgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseInMemoryDatabase($"bridge-demo-seeder-tests-{Guid.NewGuid()}")
            .Options;

        return new BridgeDbContext(options);
    }
}
