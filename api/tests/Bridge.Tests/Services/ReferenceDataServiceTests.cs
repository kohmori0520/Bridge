using Bridge.Api.Dtos.Sales;
using Bridge.Api.Services.Sales;
using Bridge.Api.Services.Skills;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class ReferenceDataServiceTests
{
    [Fact]
    public async Task SkillService_ListAsync_OrdersByCategoryThenId()
    {
        await using var db = TestDbContextFactory.Create("bridge-reference-service-tests");
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var csharp = new Skill { Name = "C#", Category = SkillCategory.Language };
        var aws = new Skill { Name = "AWS", Category = SkillCategory.Infrastructure };
        db.Skills.AddRange(react, csharp, aws);
        await db.SaveChangesAsync();
        var service = new SkillService(db);

        var result = await service.ListAsync();

        result.Select(s => s.Name).Should().Equal("C#", "React", "AWS");
        result.Select(s => s.Category).Should().Equal("Language", "Framework", "Infrastructure");
    }

    [Fact]
    public async Task SalesService_ListAsync_ReturnsSalesWithUserEmailOrderedById()
    {
        await using var db = TestDbContextFactory.Create("bridge-reference-service-tests");
        db.Sales.AddRange(
            new Sales
            {
                User = new User { Email = "sato@bridge.local", PasswordHash = "hash", Role = UserRole.Sales },
                Name = "佐藤 営業",
                Department = "Sales",
            },
            new Sales
            {
                User = new User { Email = "suzuki@bridge.local", PasswordHash = "hash", Role = UserRole.Sales },
                Name = "鈴木 営業",
                Department = "Account",
            });
        await db.SaveChangesAsync();
        var service = new SalesService(db);

        var result = await service.ListAsync();

        result.Select(s => s.Email).Should().Equal("sato@bridge.local", "suzuki@bridge.local");
        result[0].Name.Should().Be("佐藤 営業");
        result[1].Department.Should().Be("Account");
    }

    [Fact]
    public async Task SalesService_UpdateAsync_UpdatesProfileAndKeepsEmail()
    {
        await using var db = TestDbContextFactory.Create("bridge-reference-service-tests");
        var sales = new Sales
        {
            User = new User { Email = "sato@bridge.local", PasswordHash = "hash", Role = UserRole.Sales },
            Name = "旧氏名",
            Department = "Old",
        };
        db.Sales.Add(sales);
        await db.SaveChangesAsync();
        var service = new SalesService(db);

        var result = await service.UpdateAsync(sales.Id, new UpdateSalesRequest
        {
            Name = "新氏名",
            Department = "New",
        });

        result.Should().NotBeNull();
        result!.Name.Should().Be("新氏名");
        result.Department.Should().Be("New");
        result.Email.Should().Be("sato@bridge.local");

        var stored = await db.Sales.SingleAsync();
        stored.UpdatedAt.Should().BeAfter(stored.CreatedAt.AddTicks(-1));
    }
}
