using System.Security.Claims;
using Bridge.Api.Dtos.Matching;
using Bridge.Api.Services.Matching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Engineers;

[ApiController]
[Route("engineers/me")]
[Authorize(Policy = "EngineerOnly")]
public class EngineerMatchesController : ControllerBase
{
    private readonly IMatchingService _service;

    public EngineerMatchesController(IMatchingService service)
    {
        _service = service;
    }

    [HttpGet("matches")]
    [ProducesResponseType(typeof(EngineerMatchesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyMatches(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(page, 1);
        limit = Math.Clamp(limit, 1, 100);

        var engineerIdClaim = User.FindFirstValue("engineerId");
        if (!int.TryParse(engineerIdClaim, out var engineerId))
            return Forbid();

        var result = await _service.GetMatchesForEngineerAsync(engineerId, page, limit);
        return result is null ? NotFound() : Ok(result);
    }
}
