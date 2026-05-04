using Bridge.Api.Dtos.EngineerPreferences;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.EngineerPreferences;

public class EngineerPreferenceService : IEngineerPreferenceService
{
    private readonly BridgeDbContext _db;

    public EngineerPreferenceService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<EngineerPreferencesResponse?> GetAsync(int engineerId)
    {
        var engineer = await _db.Engineers
            .Include(e => e.PreferredSkills).ThenInclude(p => p.Skill)
            .Include(e => e.PreferredCategories)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == engineerId);

        if (engineer is null) return null;

        return new EngineerPreferencesResponse
        {
            PreferredSkills = engineer.PreferredSkills
                .OrderBy(p => p.Skill.Category)
                .ThenBy(p => p.Skill.Id)
                .Select(p => new PreferredSkillItem
                {
                    SkillId = p.SkillId,
                    SkillName = p.Skill.Name,
                    Category = p.Skill.Category.ToString(),
                })
                .ToList(),
            PreferredCategories = engineer.PreferredCategories
                .OrderBy(pc => pc.Category)
                .Select(pc => pc.Category.ToString())
                .ToList(),
            AvoidedWorkNote = engineer.AvoidedWorkNote,
        };
    }

    public async Task<EngineerPreferencesResponse?> UpdateAsync(int engineerId, UpdateEngineerPreferencesRequest request)
    {
        var engineer = await _db.Engineers
            .Include(e => e.PreferredSkills)
            .Include(e => e.PreferredCategories)
            .FirstOrDefaultAsync(e => e.Id == engineerId);

        if (engineer is null) return null;

        // カテゴリ文字列を enum に変換(不正値はスキップ)
        var requestedCategories = request.PreferredCategories
            .Select(c => Enum.TryParse<SkillCategory>(c, ignoreCase: true, out var v) ? (SkillCategory?)v : null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .Distinct()
            .ToHashSet();

        // 存在するスキルIDのみ受け付ける
        var requestedSkillIds = request.PreferredSkillIds.Distinct().ToList();
        var validSkillIds = await _db.Skills
            .Where(s => requestedSkillIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();
        var validSkillIdSet = validSkillIds.ToHashSet();

        await using var tx = await _db.Database.BeginTransactionAsync();

        // ---- PreferredSkills の差分更新 ----
        var existingSkillIds = engineer.PreferredSkills.Select(p => p.SkillId).ToHashSet();
        var skillsToDelete = engineer.PreferredSkills
            .Where(p => !validSkillIdSet.Contains(p.SkillId))
            .ToList();
        var skillsToAdd = validSkillIdSet
            .Where(id => !existingSkillIds.Contains(id))
            .Select(id => new EngineerPreferredSkill
            {
                EngineerId = engineerId,
                SkillId = id,
            })
            .ToList();

        if (skillsToDelete.Count > 0) _db.EngineerPreferredSkills.RemoveRange(skillsToDelete);
        if (skillsToAdd.Count > 0) await _db.EngineerPreferredSkills.AddRangeAsync(skillsToAdd);

        // ---- PreferredCategories の差分更新 ----
        var existingCategories = engineer.PreferredCategories.Select(pc => pc.Category).ToHashSet();
        var categoriesToDelete = engineer.PreferredCategories
            .Where(pc => !requestedCategories.Contains(pc.Category))
            .ToList();
        var categoriesToAdd = requestedCategories
            .Where(c => !existingCategories.Contains(c))
            .Select(c => new EngineerPreferredCategory
            {
                EngineerId = engineerId,
                Category = c,
            })
            .ToList();

        if (categoriesToDelete.Count > 0) _db.EngineerPreferredCategories.RemoveRange(categoriesToDelete);
        if (categoriesToAdd.Count > 0) await _db.EngineerPreferredCategories.AddRangeAsync(categoriesToAdd);

        // ---- AvoidedWorkNote の更新 ----
        engineer.AvoidedWorkNote = request.AvoidedWorkNote;
        engineer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await GetAsync(engineerId);
    }
}