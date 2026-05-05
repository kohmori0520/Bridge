using Bridge.Api.Dtos.Skills;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Skills;

public class SkillService : ISkillService
{
    private readonly BridgeDbContext _db;

    public SkillService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<List<SkillResponse>> ListAsync()
    {
        return await _db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Id)
            .Select(s => new SkillResponse
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category.ToString(),
            })
            .ToListAsync();
    }
}