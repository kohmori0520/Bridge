using Bridge.Api.Dtos.Projects;
using Bridge.Api.Services.Projects;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class ProjectServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesProjectWithRequestedStatusAndValidRequiredSkillsOnly()
    {
        await using var db = TestDbContextFactory.Create("bridge-project-service-tests");
        var sales = new Sales { Name = "佐藤 営業" };
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var aws = new Skill { Name = "AWS", Category = SkillCategory.Infrastructure };
        db.Sales.Add(sales);
        db.Skills.AddRange(react, aws);
        await db.SaveChangesAsync();
        var service = new ProjectService(db);

        var result = await service.CreateAsync(new CreateProjectRequest
        {
            Title = "Web アプリ開発",
            ClientName = "A社",
            Description = "Frontend project",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = "draft",
            RequiredSkills =
            {
                new ProjectRequiredSkillItem { SkillId = react.Id, Requirement = "required", RequiredYears = 3 },
                new ProjectRequiredSkillItem { SkillId = aws.Id, Requirement = "preferred", RequiredYears = 1 },
                new ProjectRequiredSkillItem { SkillId = 999, Requirement = "required", RequiredYears = 5 },
            },
        }, sales.Id);

        result.Status.Should().Be("draft");
        result.OwnerSales.Id.Should().Be(sales.Id);
        result.RequiredSkills.Select(s => (s.SkillName, s.Requirement, s.RequiredYears)).Should().BeEquivalentTo(new[]
        {
            ("React", "required", 3),
            ("AWS", "preferred", 1),
        });
        (await db.ProjectRequiredSkills.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesProjectFieldsAndRequiredSkills()
    {
        await using var db = TestDbContextFactory.Create("bridge-project-service-tests");
        var sales = new Sales { Name = "佐藤 営業" };
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var project = new Project
        {
            OwnerSales = sales,
            Title = "旧案件",
            ClientName = "A社",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 31),
            UnitPriceMin = 700000,
            UnitPriceMax = 900000,
            Status = ProjectStatus.Open,
            RequiredSkills =
            {
                new ProjectRequiredSkill
                {
                    Skill = react,
                    RequirementType = RequirementType.Required,
                    RequiredYears = 3,
                },
            },
        };
        db.Projects.Add(project);
        db.Skills.Add(typeScript);
        await db.SaveChangesAsync();
        var service = new ProjectService(db);

        var result = await service.UpdateAsync(project.Id, new UpdateProjectRequest
        {
            Title = "更新案件",
            ClientName = "B社",
            Description = "Updated",
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2027, 1, 31),
            UnitPriceMin = 750000,
            UnitPriceMax = 950000,
            Status = "closed",
            RequiredSkills =
            {
                new ProjectRequiredSkillItem { SkillId = typeScript.Id, Requirement = "required", RequiredYears = 2 },
            },
        });

        result.Should().NotBeNull();
        result!.Title.Should().Be("更新案件");
        result.ClientName.Should().Be("B社");
        result.Status.Should().Be("closed");
        result.RequiredSkills.Should().ContainSingle(s =>
            s.SkillId == typeScript.Id && s.SkillName == "TypeScript" && s.RequiredYears == 2);

        var storedProject = await db.Projects.Include(p => p.RequiredSkills).SingleAsync();
        storedProject.Status.Should().Be(ProjectStatus.Closed);
        storedProject.RequiredSkills.Should().ContainSingle(s => s.SkillId == typeScript.Id);
    }

    [Fact]
    public async Task ListAsync_FiltersByOwnerAndStatusAndCountsActiveAssignments()
    {
        await using var db = TestDbContextFactory.Create("bridge-project-service-tests");
        var owner = new Sales { Name = "佐藤 営業" };
        var otherOwner = new Sales { Name = "鈴木 営業" };
        var project = new Project
        {
            OwnerSales = owner,
            Title = "対象案件",
            ClientName = "A社",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 31),
            Status = ProjectStatus.Open,
            Assignments =
            {
                new Assignment { Engineer = new Engineer { Name = "稼働中" }, AssignedAt = new DateOnly(2026, 6, 1), Status = AssignmentStatus.Active },
                new Assignment { Engineer = new Engineer { Name = "終了済み" }, AssignedAt = new DateOnly(2026, 6, 1), Status = AssignmentStatus.Ended },
            },
        };
        db.Projects.AddRange(
            project,
            new Project
            {
                OwnerSales = owner,
                Title = "終了案件",
                ClientName = "B社",
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 12, 31),
                Status = ProjectStatus.Closed,
            },
            new Project
            {
                OwnerSales = otherOwner,
                Title = "別営業案件",
                ClientName = "C社",
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 12, 31),
                Status = ProjectStatus.Open,
            });
        await db.SaveChangesAsync();
        var service = new ProjectService(db);

        var result = await service.ListAsync(new ProjectListQuery
        {
            OwnerSalesId = owner.Id,
            Status = "open",
        });

        result.Pagination.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("対象案件");
        result.Items[0].AssignedCount.Should().Be(1);
    }
}
