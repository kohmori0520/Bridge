using System.Security.Claims;
using Bridge.Api.Dtos.EngineerPreferences;
using Bridge.Api.Services.EngineerPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Engineers;

[ApiController]
[Route("engineers")]
[Authorize]
public class EngineerPreferencesController : ControllerBase
{
    private readonly IEngineerPreferenceService _service;

    public EngineerPreferencesController(IEngineerPreferenceService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}/preferences")]
    [ProducesResponseType(typeof(EngineerPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        if (!CanAccessEngineer(id)) return Forbid();
        var result = await _service.GetAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:int}/preferences")]
    [ProducesResponseType(typeof(EngineerPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEngineerPreferencesRequest request)
    {
        if (!CanAccessEngineer(id)) return Forbid();
        var result = await _service.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("me/preferences")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(EngineerPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();
        var result = await _service.GetAsync(engineerId.Value);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("me/preferences")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(EngineerPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateEngineerPreferencesRequest request)
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();
        var result = await _service.UpdateAsync(engineerId.Value, request);
        return result is null ? NotFound() : Ok(result);
    }

    private int? GetEngineerIdFromClaims()
    {
        var claim = User.FindFirstValue("engineerId");
        return int.TryParse(claim, out var v) ? v : null;
    }

    private bool CanAccessEngineer(int targetEngineerId)
    {
        if (User.IsInRole("Sales") || User.IsInRole("Admin")) return true;

        if (User.IsInRole("Engineer"))
        {
            var myEngineerId = GetEngineerIdFromClaims();
            return myEngineerId == targetEngineerId;
        }

        return false;
    }
}
