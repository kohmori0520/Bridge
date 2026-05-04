using Bridge.Api.Services.ExpiringContracts;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Tests.Support;
using FluentAssertions;

namespace Bridge.Tests.Services;

public class ExpiringContractServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsActiveCurrentContractsExpiringWithinThresholdOrderedByDaysRemaining()
    {
        await using var db = TestDbContextFactory.Create("bridge-expiring-contract-service-tests");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetSales = new Sales { Name = "佐藤 営業" };
        var otherSales = new Sales { Name = "鈴木 営業" };
        db.Assignments.AddRange(
            CreateAssignment(targetSales, "田中 太郎", "急ぎ案件", today.AddDays(5), AssignmentStatus.Active),
            CreateAssignment(targetSales, "鈴木 花子", "余裕案件", today.AddDays(20), AssignmentStatus.Active),
            CreateAssignment(targetSales, "終了済み", "終了案件", today.AddDays(3), AssignmentStatus.Ended),
            CreateAssignment(otherSales, "別営業", "別営業案件", today.AddDays(2), AssignmentStatus.Active));
        await db.SaveChangesAsync();
        var service = new ExpiringContractService(db);

        var result = await service.GetAsync(days: 30, filterBySalesId: targetSales.Id);

        result.Items.Select(i => i.Engineer.Name).Should().Equal("田中 太郎", "鈴木 花子");
        result.Items.Select(i => i.DaysRemaining).Should().Equal(5, 20);
        result.Items.Should().OnlyContain(i => i.RenewalStatus == "not_planned");
    }

    [Fact]
    public async Task GetAsync_MarksContractAsScheduled_WhenNextContractExists()
    {
        await using var db = TestDbContextFactory.Create("bridge-expiring-contract-service-tests");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sales = new Sales { Name = "佐藤 営業" };
        var currentPeriodTo = today.AddDays(10);
        var assignment = CreateAssignment(sales, "田中 太郎", "更新予定案件", currentPeriodTo, AssignmentStatus.Active);
        assignment.Contracts.Add(new Contract
        {
            PeriodFrom = currentPeriodTo.AddDays(1),
            PeriodTo = currentPeriodTo.AddDays(90),
            UnitPrice = 850000,
            ContractType = ContractType.Renewal,
        });
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        var service = new ExpiringContractService(db);

        var result = await service.GetAsync(days: 30, filterBySalesId: null);

        result.Items.Should().ContainSingle();
        result.Items[0].RenewalStatus.Should().Be("scheduled");
        result.Items[0].DaysRemaining.Should().Be(10);
    }

    private static Assignment CreateAssignment(
        Sales sales,
        string engineerName,
        string projectTitle,
        DateOnly currentPeriodTo,
        AssignmentStatus status)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new Assignment
        {
            Engineer = new Engineer { Name = engineerName, PrimarySales = sales },
            Project = new Project
            {
                OwnerSales = sales,
                Title = projectTitle,
                ClientName = "A社",
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(120),
                UnitPriceMin = 700000,
                UnitPriceMax = 900000,
                Status = ProjectStatus.Open,
            },
            AssignedAt = today.AddDays(-30),
            Status = status,
            Contracts =
            {
                new Contract
                {
                    PeriodFrom = today.AddDays(-30),
                    PeriodTo = currentPeriodTo,
                    UnitPrice = 800000,
                    ContractType = ContractType.Initial,
                },
            },
        };
    }
}
