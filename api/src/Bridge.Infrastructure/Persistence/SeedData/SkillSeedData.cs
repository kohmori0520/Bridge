using Bridge.Domain.Entities;
using Bridge.Domain.Enums;

namespace Bridge.Infrastructure.Persistence.SeedData;

public static class SkillSeedData
{
    public static IEnumerable<Skill> GetSeedSkills()
    {
        return new List<Skill>
        {
            // Language(プログラミング言語)
            new() { Id = 1,  Name = "C#",         Category = SkillCategory.Language },
            new() { Id = 2,  Name = "TypeScript", Category = SkillCategory.Language },
            new() { Id = 3,  Name = "JavaScript", Category = SkillCategory.Language },
            new() { Id = 4,  Name = "Python",     Category = SkillCategory.Language },
            new() { Id = 5,  Name = "Java",       Category = SkillCategory.Language },
            new() { Id = 6,  Name = "Go",         Category = SkillCategory.Language },
            new() { Id = 7,  Name = "PHP",        Category = SkillCategory.Language },
            new() { Id = 8,  Name = "Ruby",       Category = SkillCategory.Language },
            new() { Id = 9,  Name = "Kotlin",     Category = SkillCategory.Language },
            new() { Id = 10, Name = "Swift",      Category = SkillCategory.Language },

            // Framework
            new() { Id = 11, Name = "React",       Category = SkillCategory.Framework },
            new() { Id = 12, Name = "Vue.js",      Category = SkillCategory.Framework },
            new() { Id = 13, Name = "Next.js",     Category = SkillCategory.Framework },
            new() { Id = 14, Name = "ASP.NET Core", Category = SkillCategory.Framework },
            new() { Id = 15, Name = "Blazor",      Category = SkillCategory.Framework },
            new() { Id = 16, Name = "Spring Boot", Category = SkillCategory.Framework },
            new() { Id = 17, Name = "Django",      Category = SkillCategory.Framework },
            new() { Id = 18, Name = "Ruby on Rails", Category = SkillCategory.Framework },
            new() { Id = 19, Name = "Laravel",     Category = SkillCategory.Framework },
            new() { Id = 20, Name = "Express",     Category = SkillCategory.Framework },

            // Infrastructure
            new() { Id = 21, Name = "AWS",        Category = SkillCategory.Infrastructure },
            new() { Id = 22, Name = "Azure",      Category = SkillCategory.Infrastructure },
            new() { Id = 23, Name = "GCP",        Category = SkillCategory.Infrastructure },
            new() { Id = 24, Name = "Docker",     Category = SkillCategory.Infrastructure },
            new() { Id = 25, Name = "Kubernetes", Category = SkillCategory.Infrastructure },
            new() { Id = 26, Name = "Terraform",  Category = SkillCategory.Infrastructure },
            new() { Id = 27, Name = "Linux",      Category = SkillCategory.Infrastructure },

            // Database
            new() { Id = 28, Name = "PostgreSQL", Category = SkillCategory.Database },
            new() { Id = 29, Name = "MySQL",      Category = SkillCategory.Database },
            new() { Id = 30, Name = "SQL Server", Category = SkillCategory.Database },
            new() { Id = 31, Name = "Oracle",     Category = SkillCategory.Database },
            new() { Id = 32, Name = "MongoDB",    Category = SkillCategory.Database },
            new() { Id = 33, Name = "Redis",      Category = SkillCategory.Database },

            // Domain(業務ドメイン)
            new() { Id = 34, Name = "金融",       Category = SkillCategory.Domain },
            new() { Id = 35, Name = "製造",       Category = SkillCategory.Domain },
            new() { Id = 36, Name = "医療",       Category = SkillCategory.Domain },
            new() { Id = 37, Name = "小売・EC",   Category = SkillCategory.Domain },
            new() { Id = 38, Name = "通信",       Category = SkillCategory.Domain },
            new() { Id = 39, Name = "公共・官公庁", Category = SkillCategory.Domain },

            // Role(工程・役割)
            new() { Id = 40, Name = "PM",         Category = SkillCategory.Role },
            new() { Id = 41, Name = "PMO",        Category = SkillCategory.Role },
            new() { Id = 42, Name = "テックリード", Category = SkillCategory.Role },
            new() { Id = 43, Name = "アーキテクト", Category = SkillCategory.Role },
            new() { Id = 44, Name = "要件定義",     Category = SkillCategory.Role },
            new() { Id = 45, Name = "基本設計",     Category = SkillCategory.Role },
            new() { Id = 46, Name = "詳細設計",     Category = SkillCategory.Role },
            new() { Id = 47, Name = "コンサルタント", Category = SkillCategory.Role },

            // ProductType(開発種別)
            new() { Id = 48, Name = "Web開発",        Category = SkillCategory.ProductType },
            new() { Id = 49, Name = "業務系システム",  Category = SkillCategory.ProductType },
            new() { Id = 50, Name = "パッケージ導入",  Category = SkillCategory.ProductType },
            new() { Id = 51, Name = "運用保守",        Category = SkillCategory.ProductType },
            new() { Id = 52, Name = "モバイルアプリ",  Category = SkillCategory.ProductType },
            new() { Id = 53, Name = "組込・IoT",       Category = SkillCategory.ProductType },
            new() { Id = 54, Name = "データ分析",      Category = SkillCategory.ProductType },

            // Tool
            new() { Id = 55, Name = "Git",      Category = SkillCategory.Tool },
            new() { Id = 56, Name = "Jira",     Category = SkillCategory.Tool },
            new() { Id = 57, Name = "Figma",    Category = SkillCategory.Tool },
            new() { Id = 58, Name = "Slack",    Category = SkillCategory.Tool },
            new() { Id = 59, Name = "GitHub",   Category = SkillCategory.Tool },
        };
    }
}