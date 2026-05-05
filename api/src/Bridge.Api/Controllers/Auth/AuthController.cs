using System.Security.Claims;
using Bridge.Api.Dtos.Auth;
using Bridge.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Auth;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response is null)
        {
            // メール存在/非存在を区別しない(列挙攻撃対策)
            return Unauthorized(new
            {
                error = new { code = "INVALID_CREDENTIALS", message = "メールアドレスまたはパスワードが正しくありません" }
            });
        }

        return Ok(response);
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

        var response = await _authService.GetMeAsync(userId);
        if (response is null)
            return Unauthorized();

        return Ok(response);
    }
}
