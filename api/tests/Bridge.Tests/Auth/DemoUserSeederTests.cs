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

        users.Should().HaveCount(11);
        users.Should().Contain(u => u.Email == "admin@bridge.local" && u.Role == UserRole.Admin);
        users.Should().Contain(u => u.Email == "sato@bridge.local" && u.Sales != null);
        users.Should().Contain(u => u.Email == "yamada.sales@bridge.local" && u.Sales != null);

        var engineer = await db.Engineers
            .Include(e => e.User)
            .Include(e => e.PrimarySales)
            .Include(e => e.Skills)
            .SingleAsync(e => e.User.Email == "tanaka@bridge.local");
        engineer.User.Email.Should().Be("tanaka@bridge.local");
        engineer.PrimarySalesId.Should().NotBeNull();
        engineer.PrimarySales!.Name.Should().Be("佐藤 営業");
        engineer.Skills.Should().NotBeEmpty();
        passwordHasher.Verify("Engineer1234!", engineer.User.PasswordHash).Should().BeTrue();

        var projects = await db.Projects.Include(p => p.RequiredSkills).ToListAsync();
        projects.Should().HaveCount(10);
        projects.Should().Contain(p => p.Title == "SaaSフロントエンド開発" && p.RequiredSkills.Count >= 3);

        var assignments = await db.Assignments.Include(a => a.Contracts).ToListAsync();
        assignments.Should().HaveCount(5);
        assignments.Should().OnlyContain(a => a.Contracts.Count > 0);
    }

    [Fact]
    public async Task SeedAsync_AddsMissingDemoData_WhenUserAlreadyExists()
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
        users.Should().Contain(u => u.Email == "existing@bridge.local");
        users.Should().Contain(u => u.Email == "admin@bridge.local");
        users.Should().Contain(u => u.Email == "tanaka@bridge.local");
        users.Should().HaveCount(12);
        (await db.Projects.CountAsync()).Should().Be(10);
    }

    private static BridgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseInMemoryDatabase($"bridge-demo-seeder-tests-{Guid.NewGuid()}")
            .Options;

        return new BridgeDbContext(options);
    }
}
