using Bridge.Api.Dtos.Projects;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Projects;

public class ProjectService : IProjectService
{
    private readonly BridgeDbContext _db;

    public ProjectService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectListResponse> ListAsync(ProjectListQuery query)
    {
        var queryable = _db.Projects
            .Include(p => p.OwnerSales)
            .Include(p => p.Assignments)
            .AsSplitQuery()
            .AsQueryable();

        if (query.OwnerSalesId.HasValue)
            queryable = queryable.Where(p => p.OwnerSalesId == query.OwnerSalesId);

        if (!string.IsNullOrEmpty(query.Status)
            && Enum.TryParse<ProjectStatus>(query.Status, ignoreCase: true, out var statusEnum))
        {
            queryable = queryable.Where(p => p.Status == statusEnum);
        }

        var total = await queryable.CountAsync();

        var projects = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync();

        var items = projects.Select(p => new ProjectSummary
        {
            Id = p.Id,
            Title = p.Title,
            ClientName = p.ClientName,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            UnitPriceMin = p.UnitPriceMin,
            UnitPriceMax = p.UnitPriceMax,
            Status = p.Status.ToString().ToLowerInvariant(),
            OwnerSales = new OwnerSalesDto { Id = p.OwnerSales.Id, Name = p.OwnerSales.Name },
            AssignedCount = p.Assignments.Count(a => a.Status == AssignmentStatus.Active),
        }).ToList();

        return new ProjectListResponse
        {
            Items = items,
            Pagination = new PaginationDto { Page = query.Page, Limit = query.Limit, Total = total },
        };
    }

    public async Task<ProjectDetailResponse?> GetByIdAsync(int id)
    {
        var project = await _db.Projects
            .Include(p => p.OwnerSales)
            .Include(p => p.RequiredSkills).ThenInclude(rs => rs.Skill)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) return null;

        return MapToDetail(project);
    }

    public async Task<ProjectDetailResponse> CreateAsync(CreateProjectRequest request, int ownerSalesId)
    {
        var validSkillIds = await GetValidSkillIdsAsync(request.RequiredSkills.Select(s => s.SkillId).ToList());

        await using var tx = await _db.Database.BeginTransactionAsync();

        var project = new Project
        {
            OwnerSalesId = ownerSalesId,
            Title = request.Title,
            ClientName = request.ClientName,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UnitPriceMin = request.UnitPriceMin,
            UnitPriceMax = request.UnitPriceMax,
            Status = ProjectStatus.Open,
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var skills = request.RequiredSkills
            .Where(s => validSkillIds.Contains(s.SkillId))
            .Select(s => new ProjectRequiredSkill
            {
                ProjectId = project.Id,
                SkillId = s.SkillId,
                RequirementType = ParseRequirement(s.Requirement),
                RequiredYears = s.RequiredYears,
            })
            .ToList();

        if (skills.Count > 0)
        {
            await _db.ProjectRequiredSkills.AddRangeAsync(skills);
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();

        return (await GetByIdAsync(project.Id))!;
    }

    public async Task<ProjectDetailResponse?> UpdateAsync(int id, UpdateProjectRequest request)
    {
        var project = await _db.Projects
            .Include(p => p.RequiredSkills)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) return null;

        if (!Enum.TryParse<ProjectStatus>(request.Status, ignoreCase: true, out var status))
            return null;

        var validSkillIds = await GetValidSkillIdsAsync(request.RequiredSkills.Select(s => s.SkillId).ToList());

        await using var tx = await _db.Database.BeginTransactionAsync();

        project.Title = request.Title;
        project.ClientName = request.ClientName;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UnitPriceMin = request.UnitPriceMin;
        project.UnitPriceMax = request.UnitPriceMax;
        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;

        // 必須スキルは全置換(差分よりシンプル、件数が少ないので問題ない判断)
        _db.ProjectRequiredSkills.RemoveRange(project.RequiredSkills);
        await _db.SaveChangesAsync();

        var newSkills = request.RequiredSkills
            .Where(s => validSkillIds.Contains(s.SkillId))
            .Select(s => new ProjectRequiredSkill
            {
                ProjectId = id,
                SkillId = s.SkillId,
                RequirementType = ParseRequirement(s.Requirement),
                RequiredYears = s.RequiredYears,
            })
            .ToList();

        if (newSkills.Count > 0)
        {
            await _db.ProjectRequiredSkills.AddRangeAsync(newSkills);
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return false;

        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ---- helpers ----
    private async Task<HashSet<int>> GetValidSkillIdsAsync(List<int> requestedIds)
    {
        var distinct = requestedIds.Distinct().ToList();
        var ids = await _db.Skills
            .Where(s => distinct.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();
        return ids.ToHashSet();
    }

    private static RequirementType ParseRequirement(string raw)
    {
        return raw.Equals("preferred", StringComparison.OrdinalIgnoreCase)
            ? RequirementType.Preferred
            : RequirementType.Required;
    } 

    private static ProjectDetailResponse MapToDetail(Project project)
    {
        return new ProjectDetailResponse
        {
            Id = project.Id,
            Title = project.Title,
            ClientName = project.ClientName,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            UnitPriceMin = project.UnitPriceMin,
            UnitPriceMax = project.UnitPriceMax,
            Status = project.Status.ToString().ToLowerInvariant(),
            OwnerSales = new OwnerSalesDto { Id = project.OwnerSales.Id, Name = project.OwnerSales.Name },
            RequiredSkills = project.RequiredSkills
                .OrderBy(rs => rs.RequirementType)
                .ThenBy(rs => rs.Skill.Category)
                .Select(rs => new RequiredSkillItem
                {
                    SkillId = rs.SkillId,
                    SkillName = rs.Skill.Name,
                    Category = rs.Skill.Category.ToString(),
                    Requirement = rs.RequirementType == RequirementType.Required ? "required" : "preferred",
                    RequiredYears = rs.RequiredYears,
                })
                .ToList(),
        };
    }
}