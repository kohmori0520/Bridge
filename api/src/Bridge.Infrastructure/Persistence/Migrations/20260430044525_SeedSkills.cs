using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "id", "category", "name" },
                values: new object[,]
                {
                    { 1, "Language", "C#" },
                    { 2, "Language", "TypeScript" },
                    { 3, "Language", "JavaScript" },
                    { 4, "Language", "Python" },
                    { 5, "Language", "Java" },
                    { 6, "Language", "Go" },
                    { 7, "Language", "PHP" },
                    { 8, "Language", "Ruby" },
                    { 9, "Language", "Kotlin" },
                    { 10, "Language", "Swift" },
                    { 11, "Framework", "React" },
                    { 12, "Framework", "Vue.js" },
                    { 13, "Framework", "Next.js" },
                    { 14, "Framework", "ASP.NET Core" },
                    { 15, "Framework", "Blazor" },
                    { 16, "Framework", "Spring Boot" },
                    { 17, "Framework", "Django" },
                    { 18, "Framework", "Ruby on Rails" },
                    { 19, "Framework", "Laravel" },
                    { 20, "Framework", "Express" },
                    { 21, "Infrastructure", "AWS" },
                    { 22, "Infrastructure", "Azure" },
                    { 23, "Infrastructure", "GCP" },
                    { 24, "Infrastructure", "Docker" },
                    { 25, "Infrastructure", "Kubernetes" },
                    { 26, "Infrastructure", "Terraform" },
                    { 27, "Infrastructure", "Linux" },
                    { 28, "Database", "PostgreSQL" },
                    { 29, "Database", "MySQL" },
                    { 30, "Database", "SQL Server" },
                    { 31, "Database", "Oracle" },
                    { 32, "Database", "MongoDB" },
                    { 33, "Database", "Redis" },
                    { 34, "Domain", "金融" },
                    { 35, "Domain", "製造" },
                    { 36, "Domain", "医療" },
                    { 37, "Domain", "小売・EC" },
                    { 38, "Domain", "通信" },
                    { 39, "Domain", "公共・官公庁" },
                    { 40, "Role", "PM" },
                    { 41, "Role", "PMO" },
                    { 42, "Role", "テックリード" },
                    { 43, "Role", "アーキテクト" },
                    { 44, "Role", "要件定義" },
                    { 45, "Role", "基本設計" },
                    { 46, "Role", "詳細設計" },
                    { 47, "Role", "コンサルタント" },
                    { 48, "ProductType", "Web開発" },
                    { 49, "ProductType", "業務系システム" },
                    { 50, "ProductType", "パッケージ導入" },
                    { 51, "ProductType", "運用保守" },
                    { 52, "ProductType", "モバイルアプリ" },
                    { 53, "ProductType", "組込・IoT" },
                    { 54, "ProductType", "データ分析" },
                    { 55, "Tool", "Git" },
                    { 56, "Tool", "Jira" },
                    { 57, "Tool", "Figma" },
                    { 58, "Tool", "Slack" },
                    { 59, "Tool", "GitHub" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 59);
        }
    }
}
