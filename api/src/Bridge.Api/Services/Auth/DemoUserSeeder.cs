using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Bridge.Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using SalesEntity = Bridge.Domain.Entities.Sales;

namespace Bridge.Api.Services.Auth;

public static class DemoUserSeeder
{
    private const string DefaultEngineerPassword = "Engineer1234!";
    private const string DefaultSalesPassword = "Sales1234!";

    public static async Task SeedAsync(BridgeDbContext db, IPasswordHasher hasher)
    {
        await EnsureSkillsAsync(db);

        var admin = await EnsureUserAsync(db, hasher, "admin@bridge.local", "Admin1234!", UserRole.Admin);
        var sales = await EnsureSalesAsync(db, hasher, "sato@bridge.local", DefaultSalesPassword, "佐藤 営業", "第一営業部");
        var secondSales = await EnsureSalesAsync(db, hasher, "yamada.sales@bridge.local", DefaultSalesPassword, "山田 営業", "第二営業部");

        await db.SaveChangesAsync();

        var skills = await db.Skills.ToDictionaryAsync(skill => skill.Name);
        var engineers = await EnsureEngineersAsync(db, hasher, sales, secondSales, skills);
        var projects = await EnsureProjectsAsync(db, sales, secondSales, skills);
        await EnsureAssignmentsAsync(db, engineers, projects);

        admin.Email = "admin@bridge.local";
        await db.SaveChangesAsync();
    }

    private static async Task EnsureSkillsAsync(BridgeDbContext db)
    {
        if (await db.Skills.AnyAsync())
        {
            return;
        }

        await db.Skills.AddRangeAsync(SkillSeedData.GetSeedSkills());
        await db.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(
        BridgeDbContext db,
        IPasswordHasher hasher,
        string email,
        string password,
        UserRole role)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = role,
        };
        db.Users.Add(user);
        return user;
    }

    private static async Task<SalesEntity> EnsureSalesAsync(
        BridgeDbContext db,
        IPasswordHasher hasher,
        string email,
        string password,
        string name,
        string department)
    {
        var sales = await db.Sales
            .Include(s => s.User)
            .SingleOrDefaultAsync(s => s.User.Email == email);

        if (sales is not null)
        {
            sales.Name = name;
            sales.Department = department;
            return sales;
        }

        sales = new SalesEntity
        {
            User = await EnsureUserAsync(db, hasher, email, password, UserRole.Sales),
            Name = name,
            Department = department,
        };
        db.Sales.Add(sales);
        return sales;
    }

    private static async Task<List<Engineer>> EnsureEngineersAsync(
        BridgeDbContext db,
        IPasswordHasher hasher,
        SalesEntity sales,
        SalesEntity secondSales,
        IReadOnlyDictionary<string, Skill> skills)
    {
        var definitions = new[]
        {
            new DemoEngineer("tanaka@bridge.local", "田中 太郎", sales, "Webフロントエンド中心のエンジニア", "パッケージ導入は避けたい",
                new[] { ("React", 5), ("TypeScript", 4), ("C#", 3), ("AWS", 2), ("PostgreSQL", 2) },
                new[] { "React", "TypeScript", "AWS", "金融", "テックリード" },
                new[] { SkillCategory.ProductType, SkillCategory.Framework }),
            new DemoEngineer("suzuki.hanako@bridge.local", "鈴木 花子", sales, "Next.js と Node.js を使ったSaaS開発が得意", "長期の常駐運用は避けたい",
                new[] { ("TypeScript", 5), ("Next.js", 3), ("React", 4), ("Express", 4), ("AWS", 2) },
                new[] { "TypeScript", "Next.js", "Web開発", "基本設計" },
                new[] { SkillCategory.ProductType, SkillCategory.Role }),
            new DemoEngineer("yamada.jiro@bridge.local", "山田 次郎", sales, "C# と Azure を中心に業務システムを担当", "短期案件は避けたい",
                new[] { ("C#", 6), ("ASP.NET Core", 4), ("React", 2), ("Azure", 3), ("SQL Server", 5) },
                new[] { "C#", "Azure", "業務系システム", "要件定義" },
                new[] { SkillCategory.Language, SkillCategory.Infrastructure }),
            new DemoEngineer("aoki.mika@bridge.local", "青木 美香", secondSales, "Python とデータ分析基盤の開発経験が豊富", "夜間バッチ監視は避けたい",
                new[] { ("Python", 5), ("Django", 3), ("GCP", 2), ("PostgreSQL", 4), ("データ分析", 3) },
                new[] { "Python", "GCP", "データ分析", "アーキテクト" },
                new[] { SkillCategory.ProductType, SkillCategory.Role }),
            new DemoEngineer("ito.ken@bridge.local", "伊藤 健", secondSales, "Java と Spring Boot のバックエンド開発を主軸に担当", "炎上案件の途中参画は避けたい",
                new[] { ("Java", 7), ("Spring Boot", 5), ("AWS", 3), ("MySQL", 5), ("Docker", 3) },
                new[] { "Java", "Spring Boot", "AWS", "製造", "基本設計" },
                new[] { SkillCategory.Language, SkillCategory.Framework }),
            new DemoEngineer("kobayashi.rena@bridge.local", "小林 玲奈", sales, "モバイルアプリとAPI連携の設計・実装を担当", "単独常駐は避けたい",
                new[] { ("Kotlin", 4), ("Swift", 3), ("TypeScript", 2), ("Git", 3), ("モバイルアプリ", 4) },
                new[] { "Kotlin", "Swift", "モバイルアプリ", "詳細設計" },
                new[] { SkillCategory.ProductType, SkillCategory.Role }),
            new DemoEngineer("watanabe.sho@bridge.local", "渡辺 翔", sales, "Go と Kubernetes を使った高負荷API基盤が得意", "古いオンプレ運用は避けたい",
                new[] { ("Go", 4), ("Kubernetes", 3), ("Docker", 5), ("GCP", 3), ("Redis", 2) },
                new[] { "Go", "Kubernetes", "GCP", "Web開発", "テックリード" },
                new[] { SkillCategory.Infrastructure, SkillCategory.Role }),
            new DemoEngineer("nakamura.yui@bridge.local", "中村 優衣", secondSales, "UI設計とReact実装を横断して担当", "要件が固まらない単発改修は避けたい",
                new[] { ("React", 4), ("TypeScript", 4), ("Figma", 3), ("Vue.js", 2), ("GitHub", 3) },
                new[] { "React", "TypeScript", "Figma", "Web開発", "詳細設計" },
                new[] { SkillCategory.Framework, SkillCategory.Tool }),
        };

        var engineers = new List<Engineer>();
        foreach (var definition in definitions)
        {
            var engineer = await EnsureEngineerAsync(db, hasher, definition, skills);
            engineers.Add(engineer);
        }

        await db.SaveChangesAsync();
        return engineers;
    }

    private static async Task<Engineer> EnsureEngineerAsync(
        BridgeDbContext db,
        IPasswordHasher hasher,
        DemoEngineer definition,
        IReadOnlyDictionary<string, Skill> skills)
    {
        var engineer = await db.Engineers
            .Include(e => e.User)
            .Include(e => e.Skills)
            .Include(e => e.PreferredSkills)
            .Include(e => e.PreferredCategories)
            .SingleOrDefaultAsync(e => e.User.Email == definition.Email);

        if (engineer is null)
        {
            engineer = new Engineer
            {
                User = await EnsureUserAsync(db, hasher, definition.Email, DefaultEngineerPassword, UserRole.Engineer),
                Name = definition.Name,
            };
            db.Engineers.Add(engineer);
        }

        engineer.Name = definition.Name;
        engineer.Bio = definition.Bio;
        engineer.AvoidedWorkNote = definition.AvoidedWorkNote;
        engineer.PrimarySales = definition.PrimarySales;

        foreach (var (skillName, years) in definition.Skills)
        {
            if (!skills.TryGetValue(skillName, out var skill))
            {
                continue;
            }

            var existing = engineer.Skills.SingleOrDefault(s => s.SkillId == skill.Id);
            if (existing is null)
            {
                engineer.Skills.Add(new EngineerSkill { SkillId = skill.Id, Years = years });
            }
            else
            {
                existing.Years = years;
            }
        }

        foreach (var skillName in definition.PreferredSkillNames)
        {
            if (skills.TryGetValue(skillName, out var skill) && engineer.PreferredSkills.All(s => s.SkillId != skill.Id))
            {
                engineer.PreferredSkills.Add(new EngineerPreferredSkill { SkillId = skill.Id });
            }
        }

        foreach (var category in definition.PreferredCategories)
        {
            if (engineer.PreferredCategories.All(c => c.Category != category))
            {
                engineer.PreferredCategories.Add(new EngineerPreferredCategory { Category = category });
            }
        }

        return engineer;
    }

    private static async Task<List<Project>> EnsureProjectsAsync(
        BridgeDbContext db,
        SalesEntity sales,
        SalesEntity secondSales,
        IReadOnlyDictionary<string, Skill> skills)
    {
        var definitions = new[]
        {
            new DemoProject("金融系Webアプリ開発", "A銀行", sales, ProjectStatus.Open, "メガバンク向けの投信管理システム刷新。React と C# を用いた画面・API 開発を担当します。", "2026-06-01", "2026-12-31", 700000, 900000,
                new[] { ("React", 3, RequirementType.Required), ("TypeScript", 2, RequirementType.Required), ("C#", 5, RequirementType.Required), ("金融", 2, RequirementType.Required), ("AWS", 1, RequirementType.Preferred) }),
            new DemoProject("SaaSフロントエンド開発", "B社", sales, ProjectStatus.Open, "エンタープライズSaaSの管理画面刷新。コンポーネント設計と状態管理の改善が中心です。", "2026-06-01", "2027-03-31", 800000, 1000000,
                new[] { ("React", 4, RequirementType.Required), ("TypeScript", 3, RequirementType.Required), ("基本設計", 2, RequirementType.Required), ("AWS", 1, RequirementType.Preferred) }),
            new DemoProject("新規Webサービス立ち上げ", "C社", sales, ProjectStatus.Open, "Next.js を使った新規サービスの初期開発。要件整理からリリースまで小規模チームで進めます。", "2026-05-15", "2026-11-30", 750000, 950000,
                new[] { ("TypeScript", 3, RequirementType.Required), ("Next.js", 2, RequirementType.Required), ("Web開発", 2, RequirementType.Required), ("Figma", 1, RequirementType.Preferred) }),
            new DemoProject("モダン基幹システム刷新", "D社", sales, ProjectStatus.Closed, "既存基幹システムを段階的に Web 化。C# と React の両方を扱うポジションです。", "2026-07-01", "2027-06-30", 700000, 900000,
                new[] { ("C#", 4, RequirementType.Required), ("ASP.NET Core", 3, RequirementType.Required), ("React", 2, RequirementType.Required), ("Azure", 1, RequirementType.Preferred) }),
            new DemoProject("製造業向け受発注API基盤", "E製作所", secondSales, ProjectStatus.Open, "Java / Spring Boot によるAPI基盤の追加開発。設計レビューと性能改善も担当します。", "2026-06-15", "2027-02-28", 850000, 1050000,
                new[] { ("Java", 5, RequirementType.Required), ("Spring Boot", 3, RequirementType.Required), ("MySQL", 3, RequirementType.Required), ("製造", 1, RequirementType.Preferred), ("Docker", 1, RequirementType.Preferred) }),
            new DemoProject("データ分析ダッシュボード構築", "Fリサーチ", secondSales, ProjectStatus.Open, "Python と GCP を使った分析基盤と可視化画面の構築。データ加工から画面実装まで担当します。", "2026-07-01", "2026-12-31", 780000, 980000,
                new[] { ("Python", 4, RequirementType.Required), ("GCP", 2, RequirementType.Required), ("PostgreSQL", 2, RequirementType.Required), ("データ分析", 2, RequirementType.Required), ("React", 1, RequirementType.Preferred) }),
            new DemoProject("ECモバイルアプリ改善", "Gショップ", sales, ProjectStatus.Open, "既存ECアプリの購入導線改善。Kotlin / Swift とAPI連携の改善が中心です。", "2026-06-10", "2026-10-31", 720000, 880000,
                new[] { ("Kotlin", 3, RequirementType.Required), ("Swift", 2, RequirementType.Required), ("モバイルアプリ", 2, RequirementType.Required), ("小売・EC", 1, RequirementType.Preferred) }),
            new DemoProject("高負荷APIプラットフォーム", "Hメディア", sales, ProjectStatus.Open, "Go と Kubernetes によるAPI基盤の増強。Observability とスケーリング改善を行います。", "2026-08-01", "2027-01-31", 900000, 1150000,
                new[] { ("Go", 3, RequirementType.Required), ("Kubernetes", 2, RequirementType.Required), ("Docker", 3, RequirementType.Required), ("Redis", 1, RequirementType.Preferred), ("GCP", 1, RequirementType.Preferred) }),
            new DemoProject("医療予約システムUI改善", "Iクリニック", secondSales, ProjectStatus.Draft, "医療機関向け予約システムのUI改善。Figma での設計と React 実装を担当します。", "2026-09-01", "2027-02-28", 680000, 850000,
                new[] { ("React", 3, RequirementType.Required), ("TypeScript", 3, RequirementType.Required), ("Figma", 2, RequirementType.Required), ("医療", 1, RequirementType.Preferred) }),
            new DemoProject("公共系クラウド移行支援", "J自治体", secondSales, ProjectStatus.Draft, "オンプレ環境からクラウドへの段階移行。Azure と Terraform の設計支援が中心です。", "2026-10-01", "2027-03-31", 820000, 1020000,
                new[] { ("Azure", 3, RequirementType.Required), ("Terraform", 2, RequirementType.Required), ("Linux", 3, RequirementType.Required), ("公共・官公庁", 1, RequirementType.Preferred) }),
        };

        var projects = new List<Project>();
        foreach (var definition in definitions)
        {
            var project = await EnsureProjectAsync(db, definition, skills);
            projects.Add(project);
        }

        await db.SaveChangesAsync();
        return projects;
    }

    private static async Task<Project> EnsureProjectAsync(
        BridgeDbContext db,
        DemoProject definition,
        IReadOnlyDictionary<string, Skill> skills)
    {
        var project = await db.Projects
            .IgnoreQueryFilters()
            .Include(p => p.RequiredSkills)
            .SingleOrDefaultAsync(p => p.Title == definition.Title);

        if (project is null)
        {
            project = new Project { Title = definition.Title };
            db.Projects.Add(project);
        }

        project.OwnerSales = definition.OwnerSales;
        project.ClientName = definition.ClientName;
        project.Description = definition.Description;
        project.Status = definition.Status;
        project.StartDate = DateOnly.Parse(definition.StartDate);
        project.EndDate = DateOnly.Parse(definition.EndDate);
        project.UnitPriceMin = definition.UnitPriceMin;
        project.UnitPriceMax = definition.UnitPriceMax;
        project.DeletedAt = null;

        foreach (var (skillName, years, requirementType) in definition.RequiredSkills)
        {
            if (!skills.TryGetValue(skillName, out var skill))
            {
                continue;
            }

            var existing = project.RequiredSkills.SingleOrDefault(s => s.SkillId == skill.Id);
            if (existing is null)
            {
                project.RequiredSkills.Add(new ProjectRequiredSkill
                {
                    SkillId = skill.Id,
                    RequiredYears = years,
                    RequirementType = requirementType,
                });
            }
            else
            {
                existing.RequiredYears = years;
                existing.RequirementType = requirementType;
            }
        }

        return project;
    }

    private static async Task EnsureAssignmentsAsync(
        BridgeDbContext db,
        IReadOnlyList<Engineer> engineers,
        IReadOnlyList<Project> projects)
    {
        var engineerByEmail = engineers.ToDictionary(e => e.User.Email);
        var projectByTitle = projects.ToDictionary(p => p.Title);

        var definitions = new[]
        {
            new DemoAssignment("tanaka@bridge.local", "金融系Webアプリ開発", "2026-03-01", "2026-03-01", "2026-05-31", 850000, ContractType.Renewal),
            new DemoAssignment("suzuki.hanako@bridge.local", "新規Webサービス立ち上げ", "2026-04-01", "2026-04-01", "2026-06-30", 870000, ContractType.Initial),
            new DemoAssignment("ito.ken@bridge.local", "製造業向け受発注API基盤", "2026-05-01", "2026-05-01", "2026-07-31", 930000, ContractType.Initial),
            new DemoAssignment("kobayashi.rena@bridge.local", "ECモバイルアプリ改善", "2026-05-10", "2026-05-10", "2026-08-31", 800000, ContractType.Initial),
            new DemoAssignment("watanabe.sho@bridge.local", "高負荷APIプラットフォーム", "2026-05-01", "2026-05-01", "2026-07-31", 1020000, ContractType.Initial),
        };

        foreach (var definition in definitions)
        {
            if (!engineerByEmail.TryGetValue(definition.EngineerEmail, out var engineer) ||
                !projectByTitle.TryGetValue(definition.ProjectTitle, out var project))
            {
                continue;
            }

            var assignment = await db.Assignments
                .Include(a => a.Contracts)
                .SingleOrDefaultAsync(a => a.EngineerId == engineer.Id && a.ProjectId == project.Id);

            if (assignment is null)
            {
                assignment = new Assignment
                {
                    Engineer = engineer,
                    Project = project,
                    AssignedAt = DateOnly.Parse(definition.AssignedAt),
                    Status = AssignmentStatus.Active,
                };
                db.Assignments.Add(assignment);
            }

            if (!assignment.Contracts.Any(c => c.PeriodFrom == DateOnly.Parse(definition.PeriodFrom) && c.PeriodTo == DateOnly.Parse(definition.PeriodTo)))
            {
                assignment.Contracts.Add(new Contract
                {
                    PeriodFrom = DateOnly.Parse(definition.PeriodFrom),
                    PeriodTo = DateOnly.Parse(definition.PeriodTo),
                    UnitPrice = definition.UnitPrice,
                    ContractType = definition.ContractType,
                });
            }
        }
    }

    private sealed record DemoEngineer(
        string Email,
        string Name,
        SalesEntity PrimarySales,
        string Bio,
        string AvoidedWorkNote,
        IReadOnlyList<(string SkillName, int Years)> Skills,
        IReadOnlyList<string> PreferredSkillNames,
        IReadOnlyList<SkillCategory> PreferredCategories);

    private sealed record DemoProject(
        string Title,
        string ClientName,
        SalesEntity OwnerSales,
        ProjectStatus Status,
        string Description,
        string StartDate,
        string EndDate,
        int UnitPriceMin,
        int UnitPriceMax,
        IReadOnlyList<(string SkillName, int Years, RequirementType RequirementType)> RequiredSkills);

    private sealed record DemoAssignment(
        string EngineerEmail,
        string ProjectTitle,
        string AssignedAt,
        string PeriodFrom,
        string PeriodTo,
        int UnitPrice,
        ContractType ContractType);
}
