using System.Security.Claims;
using Bridge.Api.Dtos.EngineerSkills;
using Bridge.Api.Dtos.Engineers;
using Bridge.Api.Services.EngineerSkills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("engineers")]
[Authorize]
public class EngineerSkillsController : ControllerBase
{
    private readonly IEngineerSkillService _service;

    public EngineerSkillsController(IEngineerSkillService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}/skills")]
    [ProducesResponseType(typeof(List<EngineerSkillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        if (!CanAccessEngineer(id)) return Forbid();
        var result = await _service.GetAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:int}/skills")]
    [ProducesResponseType(typeof(List<EngineerSkillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEngineerSkillsRequest request)
    {
        if (!CanAccessEngineer(id)) return Forbid();
        var result = await _service.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("me/skills")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(List<EngineerSkillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();
        var result = await _service.GetAsync(engineerId.Value);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("me/skills")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(List<EngineerSkillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateEngineerSkillsRequest request)
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