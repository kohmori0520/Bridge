using Bridge.Api.Dtos.Auth;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly BridgeDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        BridgeDbContext db,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Sales)
            .Include(u => u.Engineer)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(user, user.Sales?.Id, user.Engineer?.Id);

        return new LoginResponse
        {
            Token = token,
            User = new UserSummary
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                SalesId = user.Sales?.Id,
                EngineerId = user.Engineer?.Id,
            }
        };
    }

    public async Task<MeResponse?> GetMeAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Sales)
            .Include(u => u.Engineer)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return null;

        return new MeResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            SalesId = user.Sales?.Id,
            EngineerId = user.Engineer?.Id,
            Name = user.Sales?.Name ?? user.Engineer?.Name,
        };
    }
}
