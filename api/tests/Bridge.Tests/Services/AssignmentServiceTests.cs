using Bridge.Api.Dtos.Assignments;
using Bridge.Api.Services.Assignments;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class AssignmentServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesActiveAssignmentWithInitialContract()
    {
        await using var db = TestDbContextFactory.Create("bridge-assignment-service-tests");
        var fixture = await SeedEngineerAndProjectAsync(db);
        var service = new AssignmentService(db);

        var result = await service.CreateAsync(new CreateAssignmentRequest
        {
            EngineerId = fixture.EngineerId,
            ProjectId = fixture.ProjectId,
            AssignedAt = new DateOnly(2026, 5, 1),
            InitialContract = new InitialContractRequest
            {
                PeriodFrom = new DateOnly(2026, 5, 1),
                PeriodTo = new DateOnly(2026, 7, 31),
                UnitPrice = 800000,
            },
        });

        result.Success.Should().BeTrue();
        result.Assignment!.EngineerName.Should().Be("田中 太郎");
        result.Assignment.ProjectTitle.Should().Be("Web アプリ開発");
        result.Assignment.Status.Should().Be("active");
        result.Assignment.Contracts.Should().ContainSingle(c =>
            c.ContractType == "initial" && c.UnitPrice == 800000);

        var assignment = await db.Assignments.Include(a => a.Contracts).SingleAsync();
        assignment.Status.Should().Be(AssignmentStatus.Active);
        assignment.Contracts.Should().ContainSingle(c => c.ContractType == ContractType.Initial);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicateError_WhenAssignmentAlreadyExists()
    {
        await using var db = TestDbContextFactory.Create("bridge-assignment-service-tests");
        var fixture = await SeedEngineerAndProjectAsync(db);
        db.Assignments.Add(new Assignment
        {
            EngineerId = fixture.EngineerId,
            ProjectId = fixture.ProjectId,
            AssignedAt = new DateOnly(2026, 5, 1),
        });
        await db.SaveChangesAsync();
        var service = new AssignmentService(db);

        var result = await service.CreateAsync(new CreateAssignmentRequest
        {
            EngineerId = fixture.EngineerId,
            ProjectId = fixture.ProjectId,
            AssignedAt = new DateOnly(2026, 6, 1),
            InitialContract = new InitialContractRequest
            {
                PeriodFrom = new DateOnly(2026, 6, 1),
                PeriodTo = new DateOnly(2026, 8, 31),
                UnitPrice = 820000,
            },
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_ASSIGNMENT");
        (await db.Assignments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddContractAsync_AddsRenewalContract_WhenPeriodDoesNotOverlap()
    {
        await using var db = TestDbContextFactory.Create("bridge-assignment-service-tests");
        var fixture = await SeedEngineerAndProjectAsync(db);
        var assignment = new Assignment
        {
            EngineerId = fixture.EngineerId,
            ProjectId = fixture.ProjectId,
            AssignedAt = new DateOnly(2026, 5, 1),
            Contracts =
            {
                new Contract
                {
                    PeriodFrom = new DateOnly(2026, 5, 1),
                    PeriodTo = new DateOnly(2026, 7, 31),
                    UnitPrice = 800000,
                    ContractType = ContractType.Initial,
                },
            },
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        var updatedAtBefore = assignment.UpdatedAt;
        var service = new AssignmentService(db);

        var result = await service.AddContractAsync(assignment.Id, new CreateContractRequest
        {
            PeriodFrom = new DateOnly(2026, 8, 1),
            PeriodTo = new DateOnly(2026, 10, 31),
            UnitPrice = 850000,
        });

        result.Success.Should().BeTrue();
        result.Contract!.ContractType.Should().Be("renewal");
        result.Contract.UnitPrice.Should().Be(850000);
        (await db.Contracts.CountAsync(c => c.AssignmentId == assignment.Id)).Should().Be(2);

        var storedAssignment = await db.Assignments.AsNoTracking().SingleAsync(a => a.Id == assignment.Id);
        storedAssignment.UpdatedAt.Should().BeAfter(updatedAtBefore);
    }

    [Fact]
    public async Task AddContractAsync_ReturnsOverlapError_WhenPeriodOverlapsExistingContract()
    {
        await using var db = TestDbContextFactory.Create("bridge-assignment-service-tests");
        var fixture = await SeedEngineerAndProjectAsync(db);
        var assignment = new Assignment
        {
            EngineerId = fixture.EngineerId,
            ProjectId = fixture.ProjectId,
            AssignedAt = new DateOnly(2026, 5, 1),
            Contracts =
            {
                new Contract
                {
                    PeriodFrom = new DateOnly(2026, 5, 1),
                    PeriodTo = new DateOnly(2026, 7, 31),
                    UnitPrice = 800000,
                    ContractType = ContractType.Initial,
                },
            },
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        var service = new AssignmentService(db);

        var result = await service.AddContractAsync(assignment.Id, new CreateContractRequest
        {
            PeriodFrom = new DateOnly(2026, 7, 1),
            PeriodTo = new DateOnly(2026, 9, 30),
            UnitPrice = 850000,
        });

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PERIOD_OVERLAP");
        (await db.Contracts.CountAsync(c => c.AssignmentId == assignment.Id)).Should().Be(1);
    }

    private static async Task<(int EngineerId, int ProjectId)> SeedEngineerAndProjectAsync(BridgeDbContext db)
    {
        var sales = new Sales { Name = "佐藤 営業" };
        var engineer = new Engineer { Name = "田中 太郎", PrimarySales = sales };
        var project = new Project
        {
            OwnerSales = sales,
            Title = "Web アプリ開発",
            ClientName = "A社",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = ProjectStatus.Open,
        };
        db.Engineers.Add(engineer);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return (engineer.Id, project.Id);
    }
}
