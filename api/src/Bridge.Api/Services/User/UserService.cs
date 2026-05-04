using Bridge.Api.Dtos.Users;
using Bridge.Api.Services.Auth;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Users;

public class UserService : IUserService
{
    private readonly BridgeDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UserService(BridgeDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new CreateUserResult(
                Success: false,
                Response: null,
                ErrorCode: "EMAIL_ALREADY_EXISTS",
                ErrorMessage: "このメールアドレスは既に登録されています");
        }

        var role = Enum.Parse<UserRole>(request.Role);

        // User と紐付くプロフィールを1トランザクションで作成
        await using var tx = await _db.Database.BeginTransactionAsync();

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            Role = role,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        int? salesId = null;
        int? engineerId = null;

        if (role == UserRole.Sales)
        {
            var sales = new Bridge.Domain.Entities.Sales
            {
                UserId = user.Id,
                Name = request.Name,
                Department = request.Department,
            };
            _db.Sales.Add(sales);
            await _db.SaveChangesAsync();
            salesId = sales.Id;
        }
        else if (role == UserRole.Engineer)
        {
            var engineer = new Engineer
            {
                UserId = user.Id,
                Name = request.Name,
                PrimarySalesId = request.PrimarySalesId,
            };
            _db.Engineers.Add(engineer);
            await _db.SaveChangesAsync();
            engineerId = engineer.Id;
        }

        await tx.CommitAsync();

        return new CreateUserResult(
            Success: true,
            Response: new CreateUserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                SalesId = salesId,
                EngineerId = engineerId,
            },
            ErrorCode: null,
            ErrorMessage: null);
    }
}