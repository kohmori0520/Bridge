using Bridge.Api.Dtos.Users;
using Bridge.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Users;

[ApiController]
[Route("users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _service.CreateAsync(request);

        if (!result.Success)
        {
            return Conflict(new
            {
                error = new { code = result.ErrorCode, message = result.ErrorMessage }
            });
        }

        return CreatedAtAction(nameof(Create), result.Response);
    }
}
