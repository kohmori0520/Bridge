using Bridge.Api.Services.Auth;
using Bridge.Api.Services.Imports;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Bridge.Tests.Support;
using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Tests.Services;

public class ExcelImportServiceTests
{
    private static readonly PasswordHasher Hasher = new();

    [Fact]
    public async Task BuildTemplateAsync_ContainsAllSheetsAndSkillMaster()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);

        var bytes = await service.BuildTemplateAsync();

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        workbook.Worksheets.Select(w => w.Name).Should()
            .BeEquivalentTo(["技術者", "案件", "契約", "スキルマスタ"]);
        var master = workbook.Worksheet("スキルマスタ");
        master.Cell(2, 1).GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ImportAsync_CreatesEngineerProjectAndContract()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);
        var salesId = (await db.Sales.SingleAsync()).Id;

        using var stream = BuildWorkbook(
            engineers: [["tanaka@example.com", "田中太郎", "", "Web系志望", "夜間対応", "Java:5, AWS:2"]],
            projects: [["EC刷新", "ACME商事", "", "リプレイス案件", "2026-07-01", "2027-06-30", "60", "80", "Open", "Java:3", "AWS:1"]],
            contracts: [["tanaka@example.com", "EC刷新", "Active", "2026-07-01", "2026-07-01", "2026-12-31", "70", "Initial"]]);

        var result = await service.ImportAsync(stream, dryRun: false, importerSalesId: salesId);

        result.Valid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Sheets.Should().HaveCount(3);
        result.Sheets.Select(s => s.NewCount).Should().BeEquivalentTo([1, 1, 1]);

        var engineer = await db.Engineers.Include(e => e.User).Include(e => e.Skills).SingleAsync();
        engineer.Name.Should().Be("田中太郎");
        engineer.PrimarySalesId.Should().Be(salesId);
        engineer.User.Role.Should().Be(UserRole.Engineer);
        engineer.Skills.Should().HaveCount(2);

        var project = await db.Projects.Include(p => p.RequiredSkills).SingleAsync();
        project.OwnerSalesId.Should().Be(salesId);
        project.Status.Should().Be(ProjectStatus.Open);
        project.RequiredSkills.Should().HaveCount(2);
        project.RequiredSkills.Should().ContainSingle(s => s.RequirementType == RequirementType.Preferred);

        var assignment = await db.Assignments.Include(a => a.Contracts).SingleAsync();
        assignment.Status.Should().Be(AssignmentStatus.Active);
        assignment.Contracts.Should().ContainSingle(c => c.UnitPrice == 70 && c.ContractType == ContractType.Initial);

        var account = result.CreatedAccounts.Should().ContainSingle().Subject;
        account.Email.Should().Be("tanaka@example.com");
        Hasher.Verify(account.InitialPassword, engineer.User.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ImportAsync_DryRun_PersistsNothing()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);
        var salesId = (await db.Sales.SingleAsync()).Id;

        using var stream = BuildWorkbook(
            engineers: [["tanaka@example.com", "田中太郎", "", "", "", ""]]);

        var result = await service.ImportAsync(stream, dryRun: true, importerSalesId: salesId);

        result.Valid.Should().BeTrue();
        result.Sheets[0].NewCount.Should().Be(1);
        result.CreatedAccounts.Should().BeEmpty();
        db.ChangeTracker.Clear();
        (await db.Engineers.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_CollectsRowErrors_AndImportsNothing()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);
        var salesId = (await db.Sales.SingleAsync()).Id;

        using var stream = BuildWorkbook(
            engineers:
            [
                ["tanaka@example.com", "田中太郎", "", "", "", "React.js:3"],   // 存在しないスキル
                ["", "名無し", "", "", "", ""],                                  // メール欠落
            ],
            projects: [["EC刷新", "", "", "", "2026-07-01", "2026-06-30", "80", "60", "Open", "", ""]], // 日付逆転 + 単価逆転
            contracts: [["unknown@example.com", "存在しない案件", "Active", "2026-07-01", "2026-07-01", "2026-12-31", "70", "Initial"]]);

        var result = await service.ImportAsync(stream, dryRun: false, importerSalesId: salesId);

        result.Valid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(5);
        result.Errors.Should().Contain(e => e.Sheet == "技術者" && e.Message.Contains("React.js"));
        result.Errors.Should().Contain(e => e.Sheet == "技術者" && e.Row == 3 && e.Message.Contains("メールアドレスは必須"));
        result.CreatedAccounts.Should().BeEmpty();

        db.ChangeTracker.Clear();
        (await db.Engineers.CountAsync()).Should().Be(0);
        (await db.Projects.CountAsync()).Should().Be(0);
        (await db.Contracts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_IsIdempotent_OnReimport()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);
        var salesId = (await db.Sales.SingleAsync()).Id;

        using (var first = BuildWorkbook(
            engineers: [["tanaka@example.com", "田中太郎", "", "", "", "Java:5"]],
            projects: [["EC刷新", "ACME", "", "", "2026-07-01", "2027-06-30", "60", "80", "Open", "", ""]],
            contracts: [["tanaka@example.com", "EC刷新", "Active", "2026-07-01", "2026-07-01", "2026-12-31", "70", "Initial"]]))
        {
            (await service.ImportAsync(first, dryRun: false, importerSalesId: salesId)).Valid.Should().BeTrue();
        }

        db.ChangeTracker.Clear();
        var service2 = new ExcelImportService(db, Hasher);
        using var second = BuildWorkbook(
            engineers: [["tanaka@example.com", "田中次郎", "", "", "", "Java:6"]],
            projects: [["EC刷新", "ACME", "", "", "2026-07-01", "2027-06-30", "60", "80", "Open", "", ""]],
            contracts: [["tanaka@example.com", "EC刷新", "Active", "2026-07-01", "2026-07-01", "2026-12-31", "75", "Initial"]]);

        var result = await service2.ImportAsync(second, dryRun: false, importerSalesId: salesId);

        result.Valid.Should().BeTrue();
        result.Sheets.Select(s => s.NewCount).Should().BeEquivalentTo([0, 0, 0]);
        result.Sheets.Select(s => s.UpdateCount).Should().BeEquivalentTo([1, 1, 1]);
        result.CreatedAccounts.Should().BeEmpty("既存アカウントのパスワードは変更されない");

        db.ChangeTracker.Clear();
        var engineer = await db.Engineers.Include(e => e.Skills).SingleAsync();
        engineer.Name.Should().Be("田中次郎");
        engineer.Skills.Should().ContainSingle(s => s.Years == 6);
        (await db.Users.CountAsync(u => u.Role == UserRole.Engineer)).Should().Be(1);
        (await db.Contracts.SingleAsync()).UnitPrice.Should().Be(75);
    }

    [Fact]
    public async Task ImportAsync_RequiresSalesEmail_WhenImporterHasNoSalesProfile()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);

        using var stream = BuildWorkbook(
            engineers: [["tanaka@example.com", "田中太郎", "", "", "", ""]]);

        var result = await service.ImportAsync(stream, dryRun: true, importerSalesId: null);

        result.Valid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("担当営業メール"));
    }

    [Fact]
    public async Task ImportAsync_RejectsNonExcelFile()
    {
        await using var db = await CreateSeededDbAsync();
        var service = new ExcelImportService(db, Hasher);
        using var stream = new MemoryStream("これはExcelではない"u8.ToArray());

        var act = () => service.ImportAsync(stream, dryRun: true, importerSalesId: 1);

        await act.Should().ThrowAsync<InvalidImportFileException>();
    }

    // ---------------------------------------------------------------

    private static async Task<BridgeDbContext> CreateSeededDbAsync()
    {
        var db = TestDbContextFactory.Create("excel-import");

        db.Skills.AddRange(
            new Skill { Name = "Java", Category = SkillCategory.Language },
            new Skill { Name = "AWS", Category = SkillCategory.Infrastructure });

        var salesUser = new User
        {
            Email = "sato@bridge.local",
            PasswordHash = Hasher.Hash("Password123!"),
            Role = UserRole.Sales,
        };
        db.Users.Add(salesUser);
        db.Sales.Add(new Sales { User = salesUser, Name = "佐藤", Department = "第一営業部" });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static MemoryStream BuildWorkbook(
        string[][]? engineers = null, string[][]? projects = null, string[][]? contracts = null)
    {
        using var workbook = new XLWorkbook();
        AddSheet(workbook, "技術者", engineers);
        AddSheet(workbook, "案件", projects);
        AddSheet(workbook, "契約", contracts);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static void AddSheet(XLWorkbook workbook, string name, string[][]? rows)
    {
        var sheet = workbook.AddWorksheet(name);
        sheet.Cell(1, 1).Value = "ヘッダー行";
        if (rows is null) return;

        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = rows[r][c];
    }
}
