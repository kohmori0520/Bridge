using Bridge.Api.Dtos.EngineerSkills;
using Bridge.Api.Services.EngineerSkills;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class EngineerSkillServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenEngineerDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-skill-service-tests");
        var service = new EngineerSkillService(db);

        var result = await service.GetAsync(engineerId: 999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_AppliesDiff_DeduplicatesRequestAndIgnoresUnknownSkills()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-skill-service-tests");
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var aws = new Skill { Name = "AWS", Category = SkillCategory.Infrastructure };
        var engineer = new Engineer
        {
            Name = "田中 太郎",
            Skills =
            {
                new EngineerSkill { Skill = react, Years = 2 },
                new EngineerSkill { Skill = aws, Years = 1 },
            },
        };
        db.Engineers.Add(engineer);
        db.Skills.Add(typeScript);
        await db.SaveChangesAsync();
        var service = new EngineerSkillService(db);

        var result = await service.UpdateAsync(engineer.Id, new UpdateEngineerSkillsRequest
        {
            Skills =
            {
                new EngineerSkillItem { SkillId = react.Id, Years = 3 },
                new EngineerSkillItem { SkillId = react.Id, Years = 4 },
                new EngineerSkillItem { SkillId = typeScript.Id, Years = 2 },
                new EngineerSkillItem { SkillId = 999, Years = 10 },
            },
        });

        result.Should().NotBeNull();
        result!.Select(s => (s.SkillName, s.Years)).Should().BeEquivalentTo(new[]
        {
            ("TypeScript", 2),
            ("React", 4),
        });

        var storedSkills = await db.EngineerSkills
            .Where(s => s.EngineerId == engineer.Id)
            .ToListAsync();
        storedSkills.Should().HaveCount(2);
        storedSkills.Should().Contain(s => s.SkillId == react.Id && s.Years == 4);
        storedSkills.Should().Contain(s => s.SkillId == typeScript.Id && s.Years == 2);
        storedSkills.Should().NotContain(s => s.SkillId == aws.Id);
    }
}
