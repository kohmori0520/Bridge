using Bridge.Api.Services.Matching;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Matching;

public class MatchingServiceTests
{
    [Fact]
    public async Task GetMatchesForProjectAsync_OrdersAvailableEngineersByScoreAndBuildsSkillEvaluations()
    {
        await using var db = CreateDbContext();
        var fixture = await SeedProjectMatchingFixtureAsync(db);
        var service = new MatchingService(db);

        var result = await service.GetMatchesForProjectAsync(fixture.ProjectId, page: 1, limit: 20);

        result.Should().NotBeNull();
        result!.ProjectId.Should().Be(fixture.ProjectId);
        result.Pagination.Total.Should().Be(2);
        result.Matches.Select(m => m.Engineer.Id).Should().Equal(fixture.StrongEngineerId, fixture.WeakEngineerId);
        result.Matches.Should().NotContain(m => m.Engineer.Id == fixture.ActiveEngineerId);

        var weakMatch = result.Matches.Single(m => m.Engineer.Id == fixture.WeakEngineerId);
        weakMatch.SkillEvaluations.Should().Contain(e =>
            e.SkillName == "React" && e.ActualYears == 1 && e.Status == "insufficient");
        weakMatch.SkillEvaluations.Should().Contain(e =>
            e.SkillName == "TypeScript" && e.ActualYears == 0 && e.Status == "unmet");

        var secondPage = await service.GetMatchesForProjectAsync(fixture.ProjectId, page: 2, limit: 1);
        secondPage!.Pagination.Total.Should().Be(2);
        secondPage.Pagination.Page.Should().Be(2);
        secondPage.Matches.Should().ContainSingle();
        secondPage.Matches[0].Engineer.Id.Should().Be(fixture.WeakEngineerId);
    }

    [Fact]
    public async Task GetMatchesForEngineerAsync_ReturnsOpenProjectsByScore()
    {
        await using var db = CreateDbContext();
        var fixture = await SeedEngineerMatchingFixtureAsync(db);
        var service = new MatchingService(db);

        var result = await service.GetMatchesForEngineerAsync(fixture.EngineerId, page: 1, limit: 20);

        result.Should().NotBeNull();
        result!.EngineerId.Should().Be(fixture.EngineerId);
        result.Pagination.Total.Should().Be(2);
        result.Matches.Select(m => m.Project.Id).Should().Equal(fixture.StrongProjectId, fixture.WeakProjectId);
        result.Matches.Should().NotContain(m => m.Project.Id == fixture.ClosedProjectId);
        result.Matches[0].SkillEvaluations.Should().Contain(e =>
            e.SkillName == "React" && e.Status == "matched");

        var secondPage = await service.GetMatchesForEngineerAsync(fixture.EngineerId, page: 2, limit: 1);
        secondPage!.Pagination.Total.Should().Be(2);
        secondPage.Pagination.Page.Should().Be(2);
        secondPage.Matches.Should().ContainSingle();
        secondPage.Matches[0].Project.Id.Should().Be(fixture.WeakProjectId);
    }

    private static BridgeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseInMemoryDatabase($"bridge-matching-tests-{Guid.NewGuid()}")
            .Options;

        return new BridgeDbContext(options);
    }

    private static async Task<ProjectMatchingFixture> SeedProjectMatchingFixtureAsync(BridgeDbContext db)
    {
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var aws = new Skill { Name = "AWS", Category = SkillCategory.Infrastructure };
        var sales = new Sales { Name = "佐藤 営業" };
        var project = new Project
        {
            OwnerSales = sales,
            Title = "金融系Webアプリ開発",
            ClientName = "A銀行",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = ProjectStatus.Open,
            RequiredSkills =
            {
                new ProjectRequiredSkill { Skill = react, RequirementType = RequirementType.Required, RequiredYears = 3 },
                new ProjectRequiredSkill { Skill = typeScript, RequirementType = RequirementType.Required, RequiredYears = 2 },
                new ProjectRequiredSkill { Skill = aws, RequirementType = RequirementType.Preferred, RequiredYears = 1 },
            },
        };
        var strongEngineer = new Engineer
        {
            Name = "田中 太郎",
            PrimarySales = sales,
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 5 },
                new EngineerSkill { Skill = typeScript, Years = 4 },
                new EngineerSkill { Skill = aws, Years = 2 },
            },
            PreferredCategories =
            {
                new EngineerPreferredCategory { Category = SkillCategory.Framework },
            },
        };
        var weakEngineer = new Engineer
        {
            Name = "鈴木 花子",
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 1 },
            },
        };
        var activeEngineer = new Engineer
        {
            Name = "稼働中 三郎",
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 5 },
                new EngineerSkill { Skill = typeScript, Years = 5 },
                new EngineerSkill { Skill = aws, Years = 5 },
            },
            Assignments =
            {
                new Assignment { Project = project, Status = AssignmentStatus.Active, AssignedAt = new DateOnly(2026, 5, 1) },
            },
        };

        db.Projects.Add(project);
        db.Engineers.AddRange(strongEngineer, weakEngineer, activeEngineer);
        await db.SaveChangesAsync();

        return new ProjectMatchingFixture(project.Id, strongEngineer.Id, weakEngineer.Id, activeEngineer.Id);
    }

    private static async Task<EngineerMatchingFixture> SeedEngineerMatchingFixtureAsync(BridgeDbContext db)
    {
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var sales = new Sales { Name = "佐藤 営業" };
        var engineer = new Engineer
        {
            Name = "田中 太郎",
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 5 },
                new EngineerSkill { Skill = typeScript, Years = 3 },
            },
            PreferredCategories =
            {
                new EngineerPreferredCategory { Category = SkillCategory.Framework },
            },
        };
        var strongProject = new Project
        {
            OwnerSales = sales,
            Title = "React フロント開発",
            ClientName = "B社",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = ProjectStatus.Open,
            RequiredSkills =
            {
                new ProjectRequiredSkill { Skill = react, RequirementType = RequirementType.Required, RequiredYears = 3 },
                new ProjectRequiredSkill { Skill = typeScript, RequirementType = RequirementType.Required, RequiredYears = 2 },
            },
        };
        var weakProject = new Project
        {
            OwnerSales = sales,
            Title = "React 保守",
            ClientName = "C社",
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 600000,
            UnitPriceMax = 800000,
            Status = ProjectStatus.Open,
            RequiredSkills =
            {
                new ProjectRequiredSkill { Skill = react, RequirementType = RequirementType.Required, RequiredYears = 6 },
            },
        };
        var closedProject = new Project
        {
            OwnerSales = sales,
            Title = "募集終了案件",
            ClientName = "D社",
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 800000,
            UnitPriceMax = 1000000,
            Status = ProjectStatus.Closed,
            RequiredSkills =
            {
                new ProjectRequiredSkill { Skill = react, RequirementType = RequirementType.Required, RequiredYears = 1 },
            },
        };

        db.Engineers.Add(engineer);
        db.Projects.AddRange(strongProject, weakProject, closedProject);
        await db.SaveChangesAsync();

        return new EngineerMatchingFixture(engineer.Id, strongProject.Id, weakProject.Id, closedProject.Id);
    }

    private record ProjectMatchingFixture(
        int ProjectId,
        int StrongEngineerId,
        int WeakEngineerId,
        int ActiveEngineerId);

    private record EngineerMatchingFixture(
        int EngineerId,
        int StrongProjectId,
        int WeakProjectId,
        int ClosedProjectId);
}
