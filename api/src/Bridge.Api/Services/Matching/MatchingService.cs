using Bridge.Api.Dtos.Matching;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Matching;

public class MatchingService : IMatchingService
{
    private const int MaxLimit = 100;

    private readonly BridgeDbContext _db;

    public MatchingService(BridgeDbContext db)
    {
        _db = db;
    }

    // ---- 案件起点 ----
    public async Task<ProjectMatchesResponse?> GetMatchesForProjectAsync(int projectId, int page, int limit)
    {
        page = NormalizePage(page);
        limit = NormalizeLimit(limit);

        var project = await _db.Projects
            .Include(p => p.RequiredSkills).ThenInclude(rs => rs.Skill)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null) return null;

        // MVP:稼働中の技術者(Active な Assignment を持つ)は除外、空きのみ対象
        var availableEngineers = await _db.Engineers
            .Include(e => e.PrimarySales)
            .Include(e => e.Skills).ThenInclude(es => es.Skill)
            .Include(e => e.PreferredCategories)
            .Include(e => e.Assignments)
            .AsSplitQuery()
            .Where(e => !e.Assignments.Any(a => a.Status == AssignmentStatus.Active))
            .ToListAsync();

        // ---- スコア計算は MatchingScorer に委譲 ----
        var scored = availableEngineers
            .Select(e => new
            {
                Engineer = e,
                Result = MatchingScorer.Score(project, e),
            })
            .OrderByDescending(x => x.Result.Score)
            .ThenBy(x => x.Engineer.Id)
            .ToList();

        var total = scored.Count;
        var paged = scored
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        var matches = paged.Select(x => new EngineerMatch
        {
            Engineer = new EngineerSummary
            {
                Id = x.Engineer.Id,
                Name = x.Engineer.Name,
                PrimarySalesName = x.Engineer.PrimarySales?.Name ?? string.Empty,
            },
            Score = x.Result.Score,
            ScoreBreakdown = x.Result.Breakdown,
            SkillEvaluations = x.Result.SkillEvaluations,
            CategoryPreferenceMatched = x.Result.CategoryPreferenceMatched,
        }).ToList();

        return new ProjectMatchesResponse
        {
            ProjectId = project.Id,
            ProjectTitle = project.Title,
            MaxPossibleScore = 100,
            Matches = matches,
            Pagination = new PaginationDto { Page = page, Limit = limit, Total = total },
        };
    }

    // ---- エンジニア起点 ----
    public async Task<EngineerMatchesResponse?> GetMatchesForEngineerAsync(int engineerId, int page, int limit)
    {
        page = NormalizePage(page);
        limit = NormalizeLimit(limit);

        var engineer = await _db.Engineers
            .Include(e => e.Skills).ThenInclude(es => es.Skill)
            .Include(e => e.PreferredCategories)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == engineerId);

        if (engineer is null) return null;

        // 公開中の案件(Status == Open、論理削除されていないもの)
        var openProjects = await _db.Projects
            .Include(p => p.OwnerSales)
            .Include(p => p.RequiredSkills).ThenInclude(rs => rs.Skill)
            .AsSplitQuery()
            .Where(p => p.Status == ProjectStatus.Open)
            .ToListAsync();

        var scored = openProjects
            .Select(p => new
            {
                Project = p,
                Result = MatchingScorer.Score(p, engineer),
            })
            .OrderByDescending(x => x.Result.Score)
            .ThenBy(x => x.Project.Id)
            .ToList();

        var total = scored.Count;
        var paged = scored
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        var matches = paged.Select(x => new ProjectMatch
        {
            Project = new ProjectSummary
            {
                Id = x.Project.Id,
                Title = x.Project.Title,
                ClientName = x.Project.ClientName,
                UnitPriceMin = x.Project.UnitPriceMin,
                UnitPriceMax = x.Project.UnitPriceMax,
                StartDate = x.Project.StartDate,
                OwnerSalesName = x.Project.OwnerSales.Name,
            },
            Score = x.Result.Score,
            MaxPossibleScore = x.Result.MaxPossibleScore,
            ScoreBreakdown = x.Result.Breakdown,
            SkillEvaluations = x.Result.SkillEvaluations,
            CategoryPreferenceMatched = x.Result.CategoryPreferenceMatched,
        }).ToList();

        return new EngineerMatchesResponse
        {
            EngineerId = engineer.Id,
            EngineerName = engineer.Name,
            Matches = matches,
            Pagination = new PaginationDto { Page = page, Limit = limit, Total = total },
        };
    }

    private static int NormalizePage(int page)
    {
        return Math.Max(page, 1);
    }

    private static int NormalizeLimit(int limit)
    {
        return Math.Clamp(limit, 1, MaxLimit);
    }
}