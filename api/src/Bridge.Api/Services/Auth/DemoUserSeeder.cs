using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SalesEntity = Bridge.Domain.Entities.Sales;

namespace Bridge.Api.Services.Auth;

public static class DemoUserSeeder
{
    public static async Task SeedAsync(BridgeDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync())
            return;  // 既に何かいるなら何もしない

        // Admin
        var admin = new User
        {
            Email = "admin@bridge.local",
            PasswordHash = hasher.Hash("Admin1234!"),
            Role = UserRole.Admin,
        };

        // Sales
        var salesUser = new User
        {
            Email = "sato@bridge.local",
            PasswordHash = hasher.Hash("Sales1234!"),
            Role = UserRole.Sales,
        };
        var sales = new SalesEntity
        {
            User = salesUser,
            Name = "佐藤 営業",
            Department = "第一営業部",
        };

        // Engineer
        var engineerUser = new User
        {
            Email = "tanaka@bridge.local",
            PasswordHash = hasher.Hash("Engineer1234!"),
            Role = UserRole.Engineer,
        };
        var engineer = new Engineer
        {
            User = engineerUser,
            Name = "田中 太郎",
            Bio = "Webフロントエンド中心のエンジニア",
            AvoidedWorkNote = "パッケージ導入は避けたい",
        };

        db.Users.AddRange(admin, salesUser, engineerUser);
        db.Sales.Add(sales);
        db.Engineers.Add(engineer);

        await db.SaveChangesAsync();

        // Engineer の primary_sales_id を設定(salesId が確定してから)
        engineer.PrimarySalesId = sales.Id;
        await db.SaveChangesAsync();
    }
}