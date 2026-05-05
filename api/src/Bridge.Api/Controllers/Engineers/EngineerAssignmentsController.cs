using System.Security.Claims;
using Bridge.Api.Dtos.Assignments;
using Bridge.Api.Services.Assignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Engineers;

[ApiController]
[Route("engineers")]
[Authorize]
public class EngineerAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;

    public EngineerAssignmentsController(IAssignmentService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}/assignments")]
    [ProducesResponseType(typeof(EngineerAssignmentsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        if (!CanAccessEngineer(id)) return Forbid();

        var result = await _service.GetByEngineerIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("me/assignments")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(EngineerAssignmentsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();

        var result = await _service.GetByEngineerIdAsync(engineerId.Value);
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
