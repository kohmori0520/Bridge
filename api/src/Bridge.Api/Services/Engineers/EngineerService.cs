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
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (engineer is null) return null;

        return MapToDetail(engineer, today);
    }

    public async Task<EngineerListResponse> ListAsync(EngineerListQuery query)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var queryable = _db.Engineers
            .AsNoTracking()
            .AsQueryable();

        if (query.PrimarySalesId.HasValue)
            queryable = queryable.Where(e => e.PrimarySalesId == query.PrimarySalesId);

        if (query.SkillId.HasValue)
            queryable = queryable.Where(e => e.Skills.Any(s => s.SkillId == query.SkillId));

        if (query.Available == true)
            queryable = queryable.Where(e => !e.Assignments.Any(a => a.Status == AssignmentStatus.Active));
        else if (query.Available == false)
            queryable = queryable.Where(e => e.Assignments.Any(a => a.Status == AssignmentStatus.Active));

        var total = await queryable.CountAsync();
        var engineers = await queryable
            .Include(e => e.User)
            .Include(e => e.PrimarySales)
            .Include(e => e.Skills).ThenInclude(es => es.Skill)
            .Include(e => e.Assignments).ThenInclude(a => a.Project)
            .Include(e => e.Assignments).ThenInclude(a => a.Contracts)
            .AsSplitQuery()
            .OrderBy(e => e.Id)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync();

        return new EngineerListResponse
        {
            Items = engineers.Select(e => MapToSummary(e, today)).ToList(),
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

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<AssignSalesResult> AssignSalesAsync(int engineerId, int? primarySalesId)
    {
        var engineer = await _db.Engineers.FirstOrDefaultAsync(e => e.Id == engineerId);
        if (engineer is null) return AssignSalesResult.EngineerNotFound;

        if (primarySalesId.HasValue)
        {
            var salesExists = await _db.Sales.AnyAsync(s => s.Id == primarySalesId.Value);
            if (!salesExists) return AssignSalesResult.SalesNotFound;
        }

        engineer.PrimarySalesId = primarySalesId;

        await _db.SaveChangesAsync();
        return AssignSalesResult.Updated;
    }

    private static EngineerDetailResponse MapToDetail(Engineer engineer, DateOnly today)
    {
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
            CurrentContract = MapCurrentContract(engineer, today),
        };
    }

    private static EngineerSummary MapToSummary(Engineer engineer, DateOnly today)
    {
        return new EngineerSummary
        {
            Id = engineer.Id,
            Name = engineer.Name,
            Email = engineer.User?.Email ?? string.Empty,
            PrimarySales = engineer.PrimarySales == null ? null : new PrimarySalesDto
            {
                Id = engineer.PrimarySales.Id,
                Name = engineer.PrimarySales.Name,
                Department = engineer.PrimarySales.Department,
                Email = string.Empty,
            },
            IsAvailable = !engineer.Assignments.Any(a => a.Status == AssignmentStatus.Active),
            CurrentContract = MapCurrentContract(engineer, today),
            Skills = engineer.Skills
                .OrderBy(s => s.Skill.Category)
                .ThenBy(s => s.Skill.Id)
                .Select(s => new EngineerSkillDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.Skill.Name,
                    Category = s.Skill.Category.ToString(),
                    Years = s.Years,
                })
                .ToList(),
        };
    }

    private static CurrentContractDto? MapCurrentContract(Engineer engineer, DateOnly today)
    {
        var activeAssignment = engineer.Assignments
            .FirstOrDefault(a => a.Status == AssignmentStatus.Active);

        if (activeAssignment is null)
        {
            return null;
        }

        var status = ContractStatusCalculator.Calculate(activeAssignment, today);
        if (status.CurrentContract is null)
        {
            return null;
        }

        return new CurrentContractDto
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
