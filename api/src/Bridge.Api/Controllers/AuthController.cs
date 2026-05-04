using Bridge.Api.Dtos.Auth;
using Bridge.Api.Services.Auth;
using Bridge.Domain.Entities;
using Bridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly BridgeDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(
        BridgeDbContext db,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Sales)
            .Include(u => u.Engineer)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // メール存在/非存在を区別しない(列挙攻撃対策)
            return Unauthorized(new
            {
                error = new { code = "INVALID_CREDENTIALS", message = "メールアドレスまたはパスワードが正しくありません" }
            });
        }

        var token = _jwtService.GenerateToken(user, user.Sales?.Id, user.Engineer?.Id);

        return Ok(new LoginResponse
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
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // ステートレス JWT のためサーバー側 no-op。クライアントがトークンを破棄する。
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Sales)
            .Include(u => u.Engineer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Unauthorized();

        return Ok(new MeResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            SalesId = user.Sales?.Id,
            EngineerId = user.Engineer?.Id,
            Name = user.Sales?.Name ?? user.Engineer?.Name,
        });
    }
}