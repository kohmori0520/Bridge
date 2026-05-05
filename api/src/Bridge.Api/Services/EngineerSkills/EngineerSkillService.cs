using Bridge.Api.Dtos.EngineerSkills;
using Bridge.Api.Dtos.Engineers;
using Bridge.Domain.Entities;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.EngineerSkills;

public class EngineerSkillService : IEngineerSkillService
{
    private readonly BridgeDbContext _db;

    public EngineerSkillService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<List<EngineerSkillDto>?> GetAsync(int engineerId)
    {
        var engineerExists = await _db.Engineers.AnyAsync(e => e.Id == engineerId);
        if (!engineerExists) return null;

        return await _db.EngineerSkills
            .Include(es => es.Skill)
            .AsNoTracking()
            .Where(es => es.EngineerId == engineerId)
            .OrderBy(es => es.Skill.Category)
            .ThenBy(es => es.Skill.Id)
            .Select(es => new EngineerSkillDto
            {
                SkillId = es.SkillId,
                SkillName = es.Skill.Name,
                Category = es.Skill.Category.ToString(),
                Years = es.Years,
            })
            .ToListAsync();
    }

    public async Task<List<EngineerSkillDto>?> UpdateAsync(int engineerId, UpdateEngineerSkillsRequest request)
    {
        var engineerExists = await _db.Engineers.AnyAsync(e => e.Id == engineerId);
        if (!engineerExists) return null;

        // リクエスト側の重複を排除(同じ SkillId が複数回出てきても1件にする、最後の Years を採用)
        var requestedMap = request.Skills
            .GroupBy(s => s.SkillId)
            .ToDictionary(g => g.Key, g => g.Last().Years);

        // 存在するスキルIDのみ受け付ける(ガード)
        var validSkillIds = await _db.Skills
            .AsNoTracking()
            .Where(s => requestedMap.Keys.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();
        var validRequested = requestedMap
            .Where(kv => validSkillIds.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        await using var tx = await _db.Database.BeginTransactionAsync();

        // 既存を取得
        var existing = await _db.EngineerSkills
            .Where(es => es.EngineerId == engineerId)
            .ToListAsync();
        var existingMap = existing.ToDictionary(es => es.SkillId);

        // ---- 差分計算 ----
        // 1. 削除対象:既存にあって、リクエストにない
        var toDelete = existing
            .Where(es => !validRequested.ContainsKey(es.SkillId))
            .ToList();

        // 2. 追加対象:リクエストにあって、既存にない
        var toAdd = validRequested
            .Where(kv => !existingMap.ContainsKey(kv.Key))
            .Select(kv => new EngineerSkill
            {
                EngineerId = engineerId,
                SkillId = kv.Key,
                Years = kv.Value,
            })
            .ToList();

        // 3. 更新対象:両方にあるが Years が変わっている
        var toUpdate = existing
            .Where(es => validRequested.ContainsKey(es.SkillId)
                      && es.Years != validRequested[es.SkillId])
            .ToList();
        foreach (var item in toUpdate)
        {
            item.Years = validRequested[item.SkillId];
        }

        // ---- 適用 ----
        if (toDelete.Count > 0) _db.EngineerSkills.RemoveRange(toDelete);
        if (toAdd.Count > 0) await _db.EngineerSkills.AddRangeAsync(toAdd);
        // toUpdate は変更追跡で自動反映

        await _db.SaveChangesAsync();

        // 親 Engineer の UpdatedAt も触る(変更検知用)
        var engineer = await _db.Engineers.FirstAsync(e => e.Id == engineerId);
        engineer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        return await GetAsync(engineerId);
    }
}