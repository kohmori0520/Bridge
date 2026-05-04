using Bridge.Domain.Entities;
using Bridge.Domain.Enums;

namespace Bridge.Api.Services.Common;

public enum RenewalStatus
{
    Scheduled,      // 次の契約が既に存在
    NotPlanned,     // 次の契約がない
    NotApplicable,  // アサインが ended/cancelled
}

public record ContractStatus(
    int? DaysRemaining,
    RenewalStatus RenewalStatus,
    Contract? CurrentContract);

public static class ContractStatusCalculator
{
    /// <summary>
    /// アサインの現在の契約ステータスを計算する。
    /// </summary>
    /// <param name="assignment">contracts ナビゲーションプロパティをロード済みのこと</param>
    /// <param name="today">DateOnly.FromDateTime(DateTime.UtcNow) を渡す</param>
    public static ContractStatus Calculate(Assignment assignment, DateOnly today)
    {
        if (assignment.Status != AssignmentStatus.Active)
            return new ContractStatus(null, RenewalStatus.NotApplicable, null);

        var current = assignment.Contracts
            .FirstOrDefault(c => c.PeriodFrom <= today && today <= c.PeriodTo);

        if (current is null)
            return new ContractStatus(null, RenewalStatus.NotApplicable, null);

        var hasNextContract = assignment.Contracts
            .Any(c => c.PeriodFrom > current.PeriodTo);

        var daysRemaining = current.PeriodTo.DayNumber - today.DayNumber;
        var renewalStatus = hasNextContract ? RenewalStatus.Scheduled : RenewalStatus.NotPlanned;

        return new ContractStatus(daysRemaining, renewalStatus, current);
    }

    public static string ToApiString(this RenewalStatus status) => status switch
    {
        RenewalStatus.Scheduled => "scheduled",
        RenewalStatus.NotPlanned => "not_planned",
        RenewalStatus.NotApplicable => "not_applicable",
        _ => "not_applicable"
    };
}