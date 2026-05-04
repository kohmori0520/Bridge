using Bridge.Api.Dtos.EngineerPreferences;
using Bridge.Api.Services.EngineerPreferences;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class EngineerPreferenceServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsPreferredSkillsCategoriesAndAvoidedWorkNote()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-preference-service-tests");
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var engineer = new Engineer
        {
            Name = "田中 太郎",
            AvoidedWorkNote = "炎上案件は避けたい",
            PreferredSkills =
            {
                new EngineerPreferredSkill { Skill = react },
                new EngineerPreferredSkill { Skill = typeScript },
            },
            PreferredCategories =
            {
                new EngineerPreferredCategory { Category = SkillCategory.Infrastructure },
                new EngineerPreferredCategory { Category = SkillCategory.Framework },
            },
        };
        db.Engineers.Add(engineer);
        await db.SaveChangesAsync();
        var service = new EngineerPreferenceService(db);

        var result = await service.GetAsync(engineer.Id);

        result.Should().NotBeNull();
        result!.PreferredSkills.Select(s => s.SkillName).Should().Equal("TypeScript", "React");
        result.PreferredCategories.Should().Equal("Framework", "Infrastructure");
        result.AvoidedWorkNote.Should().Be("炎上案件は避けたい");
    }

    [Fact]
    public async Task UpdateAsync_ReplacesPreferencesWithValidDistinctValues()
    {
        await using var db = TestDbContextFactory.Create("bridge-engineer-preference-service-tests");
        var react = new Skill { Name = "React", Category = SkillCategory.Framework };
        var typeScript = new Skill { Name = "TypeScript", Category = SkillCategory.Language };
        var aws = new Skill { Name = "AWS", Category = SkillCategory.Infrastructure };
        var engineer = new Engineer
        {
            Name = "田中 太郎",
            PreferredSkills =
            {
                new EngineerPreferredSkill { Skill = react },
            },
            PreferredCategories =
            {
                new EngineerPreferredCategory { Category = SkillCategory.Framework },
            },
        };
        db.Engineers.Add(engineer);
        db.Skills.AddRange(typeScript, aws);
        await db.SaveChangesAsync();
        var service = new EngineerPreferenceService(db);

        var result = await service.UpdateAsync(engineer.Id, new UpdateEngineerPreferencesRequest
        {
            PreferredSkillIds = new List<int> { typeScript.Id, typeScript.Id, aws.Id, 999 },
            PreferredCategories = new List<string> { "Infrastructure", "language", "unknown" },
            AvoidedWorkNote = "長時間残業を避けたい",
        });

        result.Should().NotBeNull();
        result!.PreferredSkills.Select(s => s.SkillName).Should().Equal("TypeScript", "AWS");
        result.PreferredCategories.Should().Equal("Language", "Infrastructure");
        result.AvoidedWorkNote.Should().Be("長時間残業を避けたい");

        var storedSkillIds = await db.EngineerPreferredSkills
            .Where(p => p.EngineerId == engineer.Id)
            .Select(p => p.SkillId)
            .ToListAsync();
        storedSkillIds.Should().BeEquivalentTo(new[] { typeScript.Id, aws.Id });
    }
}
