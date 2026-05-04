using Bridge.Api.Dtos.Engineers;
using Bridge.Api.Services.Common;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Engineers;

public class EngineerService : IEngineerService
{
    private readonly BridgeDbContext _db;

    public EngineerService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<EngineerDetailResponse?> GetByIdAsync(int id)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var engineer = await _db.Engineers
            .Include(e => e.PrimarySales).ThenInclude(s => s!.User)
            .Include(e => e.Skills).ThenInclude(es => es.Skill)
            .Include(e => e.PreferredSkills).ThenInclude(eps => eps.Skill)
            .Include(e => e.PreferredCategories)
            .Include(e => e.Assignments).ThenInclude(a => a.Project)
            .Include(e => e.Assignments).ThenInclude(a => a.Contracts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (engineer is null) return null;

        return MapToDetail(engineer, today);
    }

    public async Task<EngineerListResponse> ListAsync(EngineerListQuery query)
    {
        var queryable = _db.Engineers
            .Include(e => e.PrimarySales)
            .Include(e => e.Skills).ThenInclude(es => es.Skill)
            .Include(e => e.Assignments)
            .AsSplitQuery()
            .AsQueryable();

        if (query.PrimarySalesId.HasValue)
            queryable = queryable.Where(e => e.PrimarySalesId == query.PrimarySalesId);

        if (query.SkillId.HasValue)
            queryable = queryable.Where(e => e.Skills.Any(s => s.SkillId == query.SkillId));

        // available フィルタは取得後に評価(後述)

        var total = await queryable.CountAsync();
        var engineers = await queryable
            .OrderBy(e => e.Id)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync();

        var items = engineers.Select(e => new EngineerSummary
        {
            Id = e.Id,
            Name = e.Name,
            PrimarySales = e.PrimarySales is null ? null : new PrimarySalesDto
            {
                Id = e.PrimarySales.Id,
                Name = e.PrimarySales.Name,
                Department = e.PrimarySales.Department,
                Email = string.Empty,  // 一覧では不要
            },
            IsAvailable = !e.Assignments.Any(a => a.Status == AssignmentStatus.Active),
            Skills = e.Skills.Select(s => new EngineerSkillDto
            {
                SkillId = s.SkillId,
                SkillName = s.Skill.Name,
                Category = s.Skill.Category.ToString(),
                Years = s.Years,
            }).ToList(),
        }).ToList();

        // available フィルタ
        if (query.Available == true)
            items = items.Where(i => i.IsAvailable).ToList();
        else if (query.Available == false)
            items = items.Where(i => !i.IsAvailable).ToList();

        return new EngineerListResponse
        {
            Items = items,
            Pagination = new PaginationDto
            {
                Page = query.Page,
                Limit = query.Limit,
                Total = total,
            }
        };
    }

    public async Task<EngineerDetailResponse?> UpdateAsync(int id, UpdateEngineerRequest request)
    {
        var engineer = await _db.Engineers.FirstOrDefaultAsync(e => e.Id == id);
        if (engineer is null) return null;

        engineer.Name = request.Name;
        engineer.Bio = request.Bio;
        engineer.AvoidedWorkNote = request.AvoidedWorkNote;
        engineer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private static EngineerDetailResponse MapToDetail(Engineer engineer, DateOnly today)
    {
        var activeAssignment = engineer.Assignments
            .FirstOrDefault(a => a.Status == AssignmentStatus.Active);

        CurrentContractDto? currentContract = null;
        if (activeAssignment is not null)
        {
            var status = ContractStatusCalculator.Calculate(activeAssignment, today);
            if (status.CurrentContract is not null)
            {
                currentContract = new CurrentContractDto
                {
                    AssignmentId = activeAssignment.Id,
                    ProjectId = activeAssignment.Project.Id,
                    ProjectTitle = activeAssignment.Project.Title,
                    ClientName = activeAssignment.Project.ClientName,
                    PeriodFrom = status.CurrentContract.PeriodFrom,
                    PeriodTo = status.CurrentContract.PeriodTo,
                    UnitPrice = status.CurrentContract.UnitPrice,
                    ContractType = status.CurrentContract.ContractType.ToString().ToLowerInvariant(),
                    DaysRemaining = status.DaysRemaining ?? 0,
                    RenewalStatus = status.RenewalStatus.ToApiString(),
                };
            }
        }

        return new EngineerDetailResponse
        {
            Id = engineer.Id,
            Name = engineer.Name,
            Bio = engineer.Bio,
            AvoidedWorkNote = engineer.AvoidedWorkNote,
            PrimarySales = engineer.PrimarySales is null ? null : new PrimarySalesDto
            {
                Id = engineer.PrimarySales.Id,
                Name = engineer.PrimarySales.Name,
                Department = engineer.PrimarySales.Department,
                Email = engineer.PrimarySales.User?.Email ?? string.Empty,
            },
            Skills = engineer.Skills.Select(s => new EngineerSkillDto
            {
                SkillId = s.SkillId,
                SkillName = s.Skill.Name,
                Category = s.Skill.Category.ToString(),
                Years = s.Years,
            }).ToList(),
            PreferredSkills = engineer.PreferredSkills.Select(p => new PreferredSkillDto
            {
                SkillId = p.SkillId,
                SkillName = p.Skill.Name,
                Category = p.Skill.Category.ToString(),
            }).ToList(),
            PreferredCategories = engineer.PreferredCategories
                .Select(pc => pc.Category.ToString())
                .ToList(),
            CurrentContract = currentContract,
        };
    }
}