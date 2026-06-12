using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using Bridge.Api.Dtos.Imports;
using Bridge.Api.Services.Auth;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SalesEntity = Bridge.Domain.Entities.Sales;

namespace Bridge.Api.Services.Imports;

/// <summary>
/// Excel 一括取り込み。仕様は Docs/excel-import.md を参照。
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private const string EngineerSheetName = "技術者";
    private const string ProjectSheetName = "案件";
    private const string ContractSheetName = "契約";
    private const string SkillMasterSheetName = "スキルマスタ";
    private const int MaxRowsPerSheet = 1000;

    private static readonly string[] EngineerHeaders =
        ["メールアドレス", "氏名", "担当営業メール", "自己紹介", "NG業務", "スキル"];
    private static readonly string[] ProjectHeaders =
        ["案件タイトル", "クライアント名", "担当営業メール", "概要", "開始日", "終了日", "単価下限", "単価上限", "ステータス", "必須スキル", "歓迎スキル"];
    private static readonly string[] ContractHeaders =
        ["技術者メール", "案件タイトル", "アサイン状態", "アサイン開始日", "契約開始日", "契約終了日", "単価", "契約種別"];

    private readonly BridgeDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public ExcelImportService(BridgeDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // ---------------------------------------------------------------
    // テンプレート生成
    // ---------------------------------------------------------------

    public async Task<byte[]> BuildTemplateAsync()
    {
        var skills = await _db.Skills.AsNoTracking().OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();

        using var workbook = new XLWorkbook();

        var engineerSheet = workbook.AddWorksheet(EngineerSheetName);
        WriteHeader(engineerSheet, EngineerHeaders);

        var projectSheet = workbook.AddWorksheet(ProjectSheetName);
        WriteHeader(projectSheet, ProjectHeaders);
        AddListValidation(projectSheet, column: 9, "Draft,Open,Closed,Cancelled");

        var contractSheet = workbook.AddWorksheet(ContractSheetName);
        WriteHeader(contractSheet, ContractHeaders);
        AddListValidation(contractSheet, column: 3, "Active,Ended,Cancelled");
        AddListValidation(contractSheet, column: 8, "Initial,Renewal");

        var masterSheet = workbook.AddWorksheet(SkillMasterSheetName);
        WriteHeader(masterSheet, ["スキル名", "カテゴリ"]);
        var row = 2;
        foreach (var skill in skills)
        {
            masterSheet.Cell(row, 1).Value = skill.Name;
            masterSheet.Cell(row, 2).Value = skill.Category.ToString();
            row++;
        }
        masterSheet.Cell(1, 4).Value = "※このシートは参照用です。技術者/案件シートのスキル列は「スキル名:年数」をカンマ区切りで記入してください(例: Java:5, AWS:2)";

        foreach (var sheet in workbook.Worksheets)
        {
            sheet.SheetView.FreezeRows(1);
            sheet.Columns().AdjustToContents(1, 1);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
        }
    }

    private static void AddListValidation(IXLWorksheet sheet, int column, string csvValues)
    {
        var range = sheet.Range(2, column, MaxRowsPerSheet + 1, column);
        var validation = range.CreateDataValidation();
        validation.List($"\"{csvValues}\"", true);
    }

    // ---------------------------------------------------------------
    // 取り込み
    // ---------------------------------------------------------------

    public async Task<ImportResultResponse> ImportAsync(Stream stream, bool dryRun, int? importerSalesId)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(stream);
        }
        catch (Exception ex)
        {
            throw new InvalidImportFileException("Excel ファイル(.xlsx)として読み込めませんでした", ex);
        }

        using (workbook)
        {
            var context = await ImportContext.LoadAsync(_db, importerSalesId);
            var result = new ImportResultResponse { DryRun = dryRun };

            var engineerSummary = ImportEngineers(workbook, context, result);
            var projectSummary = ImportProjects(workbook, context, result);
            var contractSummary = ImportContracts(workbook, context, result);

            result.Sheets = [engineerSummary, projectSummary, contractSummary];
            result.Valid = result.Errors.Count == 0;

            if (result.Valid && !dryRun)
            {
                await _db.SaveChangesAsync();
                result.CreatedAccounts = context.CreatedAccounts;
            }
            else if (!result.Valid)
            {
                // エラー時は何も取り込まない(全件 or なし)
                _db.ChangeTracker.Clear();
            }

            return result;
        }
    }

    // ---------------------------------------------------------------
    // 技術者シート
    // ---------------------------------------------------------------

    private ImportSheetSummary ImportEngineers(XLWorkbook workbook, ImportContext context, ImportResultResponse result)
    {
        var summary = new ImportSheetSummary { Sheet = EngineerSheetName };
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in DataRows(workbook, EngineerSheetName, result, summary))
        {
            var errorsBefore = result.Errors.Count;
            void AddError(string message) => AddRowError(result, summary, EngineerSheetName, row.RowNumber(), message);

            var email = GetString(row, 1);
            var name = GetString(row, 2);
            var salesEmail = GetString(row, 3);
            var bio = GetString(row, 4);
            var avoidedWork = GetString(row, 5);
            var skillsCell = GetString(row, 6);

            if (email.Length == 0) { AddError("メールアドレスは必須です"); continue; }
            if (!MailAddress.TryCreate(email, out _)) AddError($"メールアドレス『{email}』の形式が不正です");
            if (!seenEmails.Add(email)) AddError($"メールアドレス『{email}』がシート内で重複しています");
            if (name.Length == 0) AddError("氏名は必須です");

            var sales = ResolveSales(salesEmail, context, AddError);
            var skills = ParseSkillList(skillsCell, context, AddError);

            if (result.Errors.Count > errorsBefore) continue;

            if (context.UsersByEmail.TryGetValue(email, out var existingUser))
            {
                if (existingUser.Engineer is null)
                {
                    AddError($"『{email}』は技術者以外のロールで登録済みのため取り込めません");
                    continue;
                }

                var engineer = existingUser.Engineer;
                engineer.Name = name;
                engineer.Bio = bio;
                engineer.AvoidedWorkNote = avoidedWork;
                if (sales is not null) engineer.PrimarySalesId = sales.Id;
                if (skillsCell.Length > 0) ReplaceEngineerSkills(engineer, skills!);
                summary.UpdateCount++;
            }
            else
            {
                var initialPassword = GenerateInitialPassword();
                var user = new User
                {
                    Email = email,
                    PasswordHash = _passwordHasher.Hash(initialPassword),
                    Role = UserRole.Engineer,
                };
                var engineer = new Engineer
                {
                    User = user,
                    Name = name,
                    Bio = bio,
                    AvoidedWorkNote = avoidedWork,
                    PrimarySales = sales,
                };
                if (skillsCell.Length > 0) ReplaceEngineerSkills(engineer, skills!);

                _db.Users.Add(user);
                _db.Engineers.Add(engineer);
                user.Engineer = engineer;
                context.UsersByEmail[email] = user;
                context.CreatedAccounts.Add(new CreatedAccountResponse
                {
                    Email = email,
                    Name = name,
                    InitialPassword = initialPassword,
                });
                summary.NewCount++;
            }
        }

        return summary;
    }

    private void ReplaceEngineerSkills(Engineer engineer, IReadOnlyList<(Skill Skill, int Years)> skills)
    {
        _db.EngineerSkills.RemoveRange(engineer.Skills);
        engineer.Skills.Clear();
        foreach (var (skill, years) in skills)
        {
            engineer.Skills.Add(new EngineerSkill { Engineer = engineer, SkillId = skill.Id, Years = years });
        }
    }

    // ---------------------------------------------------------------
    // 案件シート
    // ---------------------------------------------------------------

    private ImportSheetSummary ImportProjects(XLWorkbook workbook, ImportContext context, ImportResultResponse result)
    {
        var summary = new ImportSheetSummary { Sheet = ProjectSheetName };
        var seenTitles = new HashSet<string>();

        foreach (var row in DataRows(workbook, ProjectSheetName, result, summary))
        {
            var errorsBefore = result.Errors.Count;
            void AddError(string message) => AddRowError(result, summary, ProjectSheetName, row.RowNumber(), message);

            var title = GetString(row, 1);
            var clientName = GetString(row, 2);
            var salesEmail = GetString(row, 3);
            var description = GetString(row, 4);
            var startDate = GetDate(row, 5, "開始日", AddError);
            var endDate = GetDate(row, 6, "終了日", AddError);
            var priceMin = GetInt(row, 7, "単価下限", AddError);
            var priceMax = GetInt(row, 8, "単価上限", AddError);
            var statusText = GetString(row, 9);
            var requiredSkillsCell = GetString(row, 10);
            var preferredSkillsCell = GetString(row, 11);

            if (title.Length == 0) { AddError("案件タイトルは必須です"); continue; }
            if (!seenTitles.Add(title)) AddError($"案件タイトル『{title}』がシート内で重複しています");

            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
                AddError("終了日は開始日以降にしてください");
            if (priceMin.HasValue && priceMax.HasValue && priceMin > priceMax)
                AddError("単価下限は単価上限以下にしてください");

            ProjectStatus status = default;
            if (statusText.Length == 0)
                AddError("ステータスは必須です");
            else if (!Enum.TryParse(statusText, ignoreCase: true, out status))
                AddError($"ステータス『{statusText}』は Draft / Open / Closed / Cancelled のいずれかにしてください");

            var sales = ResolveSales(salesEmail, context, AddError);
            var requiredSkills = ParseSkillList(requiredSkillsCell, context, AddError);
            var preferredSkills = ParseSkillList(preferredSkillsCell, context, AddError);

            if (result.Errors.Count > errorsBefore) continue;

            if (!context.ProjectsByTitle.TryGetValue(title, out var project))
            {
                project = new Project { Title = title };
                _db.Projects.Add(project);
                context.ProjectsByTitle[title] = project;
                summary.NewCount++;
            }
            else
            {
                summary.UpdateCount++;
            }

            project.ClientName = clientName;
            project.Description = description;
            project.StartDate = startDate!.Value;
            project.EndDate = endDate!.Value;
            project.UnitPriceMin = priceMin!.Value;
            project.UnitPriceMax = priceMax!.Value;
            project.Status = status;
            if (sales is not null)
            {
                project.OwnerSales = sales;
                project.OwnerSalesId = sales.Id;
            }

            if (requiredSkillsCell.Length > 0 || preferredSkillsCell.Length > 0)
            {
                _db.ProjectRequiredSkills.RemoveRange(project.RequiredSkills);
                project.RequiredSkills.Clear();
                foreach (var (skill, years) in requiredSkills!)
                    project.RequiredSkills.Add(new ProjectRequiredSkill
                    {
                        Project = project, SkillId = skill.Id, RequirementType = RequirementType.Required, RequiredYears = years,
                    });
                foreach (var (skill, years) in preferredSkills!)
                    project.RequiredSkills.Add(new ProjectRequiredSkill
                    {
                        Project = project, SkillId = skill.Id, RequirementType = RequirementType.Preferred, RequiredYears = years,
                    });
            }
        }

        return summary;
    }

    // ---------------------------------------------------------------
    // 契約シート
    // ---------------------------------------------------------------

    private ImportSheetSummary ImportContracts(XLWorkbook workbook, ImportContext context, ImportResultResponse result)
    {
        var summary = new ImportSheetSummary { Sheet = ContractSheetName };

        foreach (var row in DataRows(workbook, ContractSheetName, result, summary))
        {
            var errorsBefore = result.Errors.Count;
            void AddError(string message) => AddRowError(result, summary, ContractSheetName, row.RowNumber(), message);

            var engineerEmail = GetString(row, 1);
            var projectTitle = GetString(row, 2);
            var statusText = GetString(row, 3);
            var assignedAt = GetDate(row, 4, "アサイン開始日", AddError);
            var periodFrom = GetDate(row, 5, "契約開始日", AddError);
            var periodTo = GetDate(row, 6, "契約終了日", AddError);
            var unitPrice = GetInt(row, 7, "単価", AddError);
            var contractTypeText = GetString(row, 8);

            Engineer? engineer = null;
            if (engineerEmail.Length == 0)
                AddError("技術者メールは必須です");
            else if (!context.UsersByEmail.TryGetValue(engineerEmail, out var user) || user.Engineer is null)
                AddError($"技術者『{engineerEmail}』が見つかりません(技術者シートか登録済みデータに必要)");
            else
                engineer = user.Engineer;

            Project? project = null;
            if (projectTitle.Length == 0)
                AddError("案件タイトルは必須です");
            else if (!context.ProjectsByTitle.TryGetValue(projectTitle, out project))
                AddError($"案件『{projectTitle}』が見つかりません(案件シートか登録済みデータに必要)");

            AssignmentStatus status = default;
            if (statusText.Length == 0)
                AddError("アサイン状態は必須です");
            else if (!Enum.TryParse(statusText, ignoreCase: true, out status))
                AddError($"アサイン状態『{statusText}』は Active / Ended / Cancelled のいずれかにしてください");

            ContractType contractType = default;
            if (contractTypeText.Length == 0)
                AddError("契約種別は必須です");
            else if (!Enum.TryParse(contractTypeText, ignoreCase: true, out contractType))
                AddError($"契約種別『{contractTypeText}』は Initial / Renewal のいずれかにしてください");

            if (periodFrom.HasValue && periodTo.HasValue && periodTo < periodFrom)
                AddError("契約終了日は契約開始日以降にしてください");
            if (unitPrice is <= 0)
                AddError("単価は 1 以上の整数にしてください");

            if (result.Errors.Count > errorsBefore) continue;

            var assignment = context.FindAssignment(engineer!, project!, assignedAt!.Value);
            if (assignment is null)
            {
                assignment = new Assignment
                {
                    Engineer = engineer!,
                    Project = project!,
                    AssignedAt = assignedAt.Value,
                };
                _db.Assignments.Add(assignment);
                context.RegisterAssignment(assignment);
            }
            assignment.Status = status;

            var contract = assignment.Contracts.FirstOrDefault(c => c.PeriodFrom == periodFrom!.Value);
            if (contract is null)
            {
                contract = new Contract { Assignment = assignment, PeriodFrom = periodFrom!.Value };
                assignment.Contracts.Add(contract);
                _db.Contracts.Add(contract);
                summary.NewCount++;
            }
            else
            {
                summary.UpdateCount++;
            }

            contract.PeriodTo = periodTo!.Value;
            contract.UnitPrice = unitPrice!.Value;
            contract.ContractType = contractType;
        }

        return summary;
    }

    // ---------------------------------------------------------------
    // 共通パース処理
    // ---------------------------------------------------------------

    private static IEnumerable<IXLRow> DataRows(
        XLWorkbook workbook, string sheetName, ImportResultResponse result, ImportSheetSummary summary)
    {
        if (!workbook.TryGetWorksheet(sheetName, out var sheet))
        {
            AddRowError(result, summary, sheetName, 0, $"シート『{sheetName}』が見つかりません(テンプレートのシート名を変更しないでください)");
            yield break;
        }

        var rows = sheet.RowsUsed()
            .Where(r => r.RowNumber() > 1 && !r.CellsUsed().All(c => c.GetString().Trim().Length == 0))
            .ToList();

        if (rows.Count > MaxRowsPerSheet)
        {
            AddRowError(result, summary, sheetName, 0, $"行数が上限({MaxRowsPerSheet}行)を超えています");
            yield break;
        }

        foreach (var row in rows)
            yield return row;
    }

    private static void AddRowError(
        ImportResultResponse result, ImportSheetSummary summary, string sheet, int row, string message)
    {
        result.Errors.Add(new ImportRowError { Sheet = sheet, Row = row, Message = message });
        summary.ErrorCount++;
    }

    private static string GetString(IXLRow row, int column) => row.Cell(column).GetString().Trim();

    private static DateOnly? GetDate(IXLRow row, int column, string label, Action<string> addError)
    {
        var cell = row.Cell(column);
        if (cell.TryGetValue<DateTime>(out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        var text = cell.GetString().Trim();
        if (text.Length == 0)
        {
            addError($"{label}は必須です");
            return null;
        }

        string[] formats = ["yyyy-MM-dd", "yyyy/MM/dd", "yyyy/M/d"];
        if (DateOnly.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        addError($"{label}『{text}』を日付として解釈できません(yyyy-MM-dd または yyyy/MM/dd)");
        return null;
    }

    private static int? GetInt(IXLRow row, int column, string label, Action<string> addError)
    {
        var cell = row.Cell(column);
        if (cell.TryGetValue<int>(out var value))
            return value;

        var text = cell.GetString().Trim();
        if (text.Length == 0)
        {
            addError($"{label}は必須です");
            return null;
        }

        if (int.TryParse(text, out value))
            return value;

        addError($"{label}『{text}』を整数として解釈できません");
        return null;
    }

    private static SalesEntity? ResolveSales(string salesEmail, ImportContext context, Action<string> addError)
    {
        if (salesEmail.Length == 0)
        {
            if (context.ImporterSales is null)
                addError("担当営業メールが空欄です(Admin で実行する場合は担当営業メールが必須です)");
            return context.ImporterSales;
        }

        if (context.UsersByEmail.TryGetValue(salesEmail, out var user) && user.Sales is not null)
            return user.Sales;

        addError($"担当営業『{salesEmail}』が営業ユーザーとして見つかりません");
        return null;
    }

    /// <summary>「スキル名:年数」カンマ区切りをパースする。エラー時は null。空セルは空リスト。</summary>
    private static List<(Skill Skill, int Years)>? ParseSkillList(
        string cell, ImportContext context, Action<string> addError)
    {
        var results = new List<(Skill, int)>();
        if (cell.Length == 0) return results;

        var hasError = false;
        var tokens = cell.Split([',', '、', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            var parts = token.Split([':', ':'], StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var years) || years < 0 || years > 50)
            {
                addError($"スキル『{token}』は「スキル名:年数」の形式で記入してください(例: Java:5)");
                hasError = true;
                continue;
            }

            if (!context.SkillsByName.TryGetValue(parts[0], out var skill))
            {
                addError($"スキル『{parts[0]}』はスキルマスタに存在しません(テンプレートの「スキルマスタ」シート参照)");
                hasError = true;
                continue;
            }

            results.Add((skill, years));
        }

        return hasError ? null : results;
    }

    private static string GenerateInitialPassword()
    {
        // 紛らわしい文字(0/O, 1/l/I)を除いた英数字 14 桁
        const string alphabet = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return string.Create(14, alphabet, static (span, chars) =>
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        });
    }

    // ---------------------------------------------------------------
    // 取り込みコンテキスト(DB 上の既存データ + シートで新規作成された entity の索引)
    // ---------------------------------------------------------------

    private sealed class ImportContext
    {
        public required Dictionary<string, Skill> SkillsByName { get; init; }
        public required Dictionary<string, User> UsersByEmail { get; init; }
        public required Dictionary<string, Project> ProjectsByTitle { get; init; }
        public required List<Assignment> Assignments { get; init; }
        public SalesEntity? ImporterSales { get; init; }
        public List<CreatedAccountResponse> CreatedAccounts { get; } = [];

        public static async Task<ImportContext> LoadAsync(BridgeDbContext db, int? importerSalesId)
        {
            var skills = await db.Skills.ToListAsync();
            var users = await db.Users
                .Include(u => u.Engineer)!.ThenInclude(e => e!.Skills)
                .Include(u => u.Sales)
                .ToListAsync();
            var projects = await db.Projects
                .Include(p => p.RequiredSkills)
                .Where(p => p.DeletedAt == null)
                .ToListAsync();
            var assignments = await db.Assignments
                .Include(a => a.Contracts)
                .ToListAsync();
            var importerSales = importerSalesId is null
                ? null
                : await db.Sales.FirstOrDefaultAsync(s => s.Id == importerSalesId);

            return new ImportContext
            {
                SkillsByName = skills.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase),
                UsersByEmail = users.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase),
                ProjectsByTitle = projects.ToDictionary(p => p.Title),
                Assignments = assignments,
                ImporterSales = importerSales,
            };
        }

        public Assignment? FindAssignment(Engineer engineer, Project project, DateOnly assignedAt)
            => Assignments.FirstOrDefault(a =>
                a.AssignedAt == assignedAt
                && (ReferenceEquals(a.Engineer, engineer) || (engineer.Id != 0 && a.EngineerId == engineer.Id))
                && (ReferenceEquals(a.Project, project) || (project.Id != 0 && a.ProjectId == project.Id)));

        public void RegisterAssignment(Assignment assignment) => Assignments.Add(assignment);
    }
}
