using Bridge.Api.Dtos.Engineers;
using Bridge.Api.Services.Engineers;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class EngineerServiceTests
{
    [Fact]
    public async Task ListAsync_FiltersByPrimarySalesAndSkillAndMarksAvailability()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var sales = new Sales { Name = "佐藤 営業", Department = "Sales" };
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var availableEngineer = new Engineer
        {
            Name = "田中 太郎",
            User = new User { Email = "tanaka@bridge.local", PasswordHash = "hash", Role = UserRole.Engineer },
            PrimarySales = sales,
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 3 },
            },
        };
        var busyEngineer = new Engineer
        {
            Name = "鈴木 花子",
            User = new User { Email = "suzuki@bridge.local", PasswordHash = "hash", Role = UserRole.Engineer },
            PrimarySales = sales,
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 5 },
                new EngineerSkill { Skill = typeScript, Years = 4 },
            },
            Assignments =
            {
                new Assignment
                {
                    Project = CreateProject(sales, "稼働中案件"),
                    AssignedAt = new DateOnly(2026, 5, 1),
                    Status = AssignmentStatus.Active,
                    Contracts =
                    {
                        new Contract
                        {
                            PeriodFrom = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5),
                            PeriodTo = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(25),
                            UnitPrice = 900000,
                            ContractType = ContractType.Initial,
                        },
                    },
                },
            },
        };
        db.Engineers.AddRange(availableEngineer, busyEngineer);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.ListAsync(new EngineerListQuery
        {
            PrimarySalesId = sales.Id,
            SkillId = react.Id,
            Available = false,
        });

        result.Items.Should().ContainSingle();
        result.Pagination.Total.Should().Be(1);
        result.Items[0].Name.Should().Be("鈴木 花子");
        result.Items[0].Email.Should().Be("suzuki@bridge.local");
        result.Items[0].IsAvailable.Should().BeFalse();
        var currentContract = result.Items[0].CurrentContract;
        currentContract.Should().NotBeNull();
        currentContract!.ProjectTitle.Should().Be("稼働中案件");
        currentContract.UnitPrice.Should().Be(900000);
        result.Items[0].Skills.Select(s => s.SkillName).Should().Contain(new[] { "React", "TypeScript" });
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCurrentContractForActiveAssignment()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var salesUser = new User { Email = "sato@bridge.local", Role = UserRole.Sales, PasswordHash = "hash" };
        var sales = new Sales { User = salesUser, Name = "佐藤 営業", Department = "Sales" };
        var engineer = new Engineer
        {
            Name = "田中 太郎",
            PrimarySales = sales,
            Assignments =
            {
                new Assignment
                {
                    Project = CreateProject(sales, "現行案件"),
                    AssignedAt = today.AddDays(-10),
                    Status = AssignmentStatus.Active,
                    Contracts =
                    {
                        new Contract
                        {
                            PeriodFrom = today.AddDays(-10),
                            PeriodTo = today.AddDays(20),
                            UnitPrice = 800000,
                            ContractType = ContractType.Initial,
                        },
                    },
                },
            },
        };
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.GetByIdAsync(engineer.Id);

        result.Should().NotBeNull();
        result!.PrimarySales!.Email.Should().Be("sato@bridge.local");
        result.CurrentContract.Should().NotBeNull();
        result.CurrentContract!.ProjectTitle.Should().Be("現行案件");
        result.CurrentContract.DaysRemaining.Should().Be(20);
        result.CurrentContract.UnitPrice.Should().Be(800000);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBasicProfileFields()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var engineer = new Engineer
        {
            Name = "旧氏名",
            Bio = "old",
            AvoidedWorkNote = "old note",
        };
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.UpdateAsync(engineer.Id, new UpdateEngineerRequest
        {
            Name = "新氏名",
            Bio = "new bio",
            AvoidedWorkNote = "new note",
        });

        result.Should().NotBeNull();
        result!.Name.Should().Be("新氏名");
        result.Bio.Should().Be("new bio");
        result.AvoidedWorkNote.Should().Be("new note");

        var stored = await db.Engineers.SingleAsync();
        stored.UpdatedAt.Should().BeAfter(stored.CreatedAt.AddTicks(-1));
    }

    [Fact]
    public async Task AssignSalesAsync_UpdatesPrimarySales_WhenSalesExists()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var oldSales = new Sales { Name = "旧担当", Department = "Sales" };
        var newSales = new Sales { Name = "新担当", Department = "Sales" };
        var engineer = new Engineer { Name = "田中 太郎", PrimarySales = oldSales };
        db.Engineers.Add(engineer);
        db.Sales.Add(newSales);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.AssignSalesAsync(engineer.Id, newSales.Id);

        result.Should().Be(AssignSalesResult.Updated);
        var stored = await db.Engineers.SingleAsync(e => e.Id == engineer.Id);
        stored.PrimarySalesId.Should().Be(newSales.Id);
    }

    [Fact]
    public async Task AssignSalesAsync_ClearsPrimarySales_WhenIdIsNull()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var sales = new Sales { Name = "営業", Department = "Sales" };
        var engineer = new Engineer { Name = "田中 太郎", PrimarySales = sales };
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.AssignSalesAsync(engineer.Id, null);

        result.Should().Be(AssignSalesResult.Updated);
        var stored = await db.Engineers.SingleAsync(e => e.Id == engineer.Id);
        stored.PrimarySalesId.Should().BeNull();
    }

    [Fact]
    public async Task AssignSalesAsync_ReturnsEngineerNotFound_WhenEngineerMissing()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var service = new EngineerService(db);

        var result = await service.AssignSalesAsync(9999, null);

        result.Should().Be(AssignSalesResult.EngineerNotFound);
    }

    [Fact]
    public async Task AssignSalesAsync_ReturnsSalesNotFound_WhenSalesMissing()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-service-tests");
        var engineer = new Engineer { Name = "田中 太郎" };
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();
        var service = new EngineerService(db);

        var result = await service.AssignSalesAsync(engineer.Id, 9999);

        result.Should().Be(AssignSalesResult.SalesNotFound);
    }

    private static Project CreateProject(Sales sales, string title)
    {
        return new Project
        {
            OwnerSales = sales,
            Title = title,
            ClientName = "A社",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = ProjectStatus.Open,
        };
    }
}
