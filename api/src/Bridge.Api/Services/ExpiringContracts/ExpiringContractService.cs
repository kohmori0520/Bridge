using Bridge.Api.Dtos.ExpiringContracts;
using Bridge.Api.Services.Common;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.ExpiringContracts;

public class ExpiringContractService : IExpiringContractService
{
    private readonly BridgeDbContext _db;

    public ExpiringContractService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<ExpiringContractsResponse> GetAsync(int days, int? filterBySalesId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(days);

        // ---- 段階1:DB側で粗く絞る ----
        // active なアサインで、現在有効な契約の period_to が threshold 以内のもの
        var query = _db.Assignments
            .Include(a => a.Engineer)
            .Include(a => a.Project)
            .Include(a => a.Contracts)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(a => a.Status == AssignmentStatus.Active);

        if (filterBySalesId.HasValue)
        {
            query = query.Where(a => a.Engineer.PrimarySalesId == filterBySalesId.Value);
        }

        // 「終了予定が threshold 以内の契約を持つアサイン」を絞る
        // 注意:今日有効な契約だけ対象。過去契約や未来契約は除外
        var candidates = await query
            .Where(a => a.Contracts.Any(c =>
                c.PeriodFrom <= today && today <= c.PeriodTo
                && c.PeriodTo <= threshold))
            .ToListAsync();

        // ---- 段階2:メモリ側で精密判定 ----
        // ContractStatusCalculator で renewalStatus を計算し、not_applicable は除外
        var items = new List<ExpiringContractItem>();
        foreach (var assignment in candidates)
        {
            var status = ContractStatusCalculator.Calculate(assignment, today);

            // not_applicable は除外(API 設計書 5.5 節)
            if (status.RenewalStatus == RenewalStatus.NotApplicable) continue;
            if (status.CurrentContract is null) continue;
            if (status.DaysRemaining is null || status.DaysRemaining > days) continue;

            items.Add(new ExpiringContractItem
            {
                ContractId = status.CurrentContract.Id,
                AssignmentId = assignment.Id,
                Engineer = new ExpiringEngineerDto
                {
                    Id = assignment.Engineer.Id,
                    Name = assignment.Engineer.Name,
                },
                Project = new ExpiringProjectDto
                {
                    Id = assignment.Project.Id,
                    Title = assignment.Project.Title,
                    ClientName = assignment.Project.ClientName,
                },
                PeriodTo = status.CurrentContract.PeriodTo,
                DaysRemaining = status.DaysRemaining.Value,
                RenewalStatus = status.RenewalStatus.ToApiString(),
            });
        }

        // 残日数が少ない順にソート(緊急度高い順)
        items = items.OrderBy(i => i.DaysRemaining).ToList();

        return new ExpiringContractsResponse { Items = items };
    }
}