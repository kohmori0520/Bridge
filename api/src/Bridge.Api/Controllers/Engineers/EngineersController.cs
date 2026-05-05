using System.Security.Claims;
using Bridge.Api.Dtos.Engineers;
using Bridge.Api.Services.Engineers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Engineers;

[ApiController]
[Route("engineers")]
[Authorize]
public class EngineersController : ControllerBase
{
    private readonly IEngineerService _service;

    public EngineersController(IEngineerService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(EngineerListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? primarySalesId,
        [FromQuery] bool? available,
        [FromQuery] int? skillId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        // primarySalesId は "me" or 数値
        int? resolvedPrimarySalesId = ResolvePrimarySalesId(primarySalesId);

        var query = new EngineerListQuery
        {
            PrimarySalesId = resolvedPrimarySalesId,
            Available = available,
            SkillId = skillId,
            Page = page,
            Limit = limit,
        };

        var result = await _service.ListAsync(query);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(EngineerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();

        var result = await _service.GetByIdAsync(engineerId.Value);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPatch("me")]
    [Authorize(Policy = "EngineerOnly")]
    [ProducesResponseType(typeof(EngineerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateEngineerRequest request)
    {
        var engineerId = GetEngineerIdFromClaims();
        if (engineerId is null) return Forbid();

        var result = await _service.UpdateAsync(engineerId.Value, request);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EngineerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        // 認可:Sales/Admin は全員参照可、Engineer は自分のみ参照可
        if (!CanAccessEngineer(id)) return Forbid();

        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(EngineerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEngineerRequest request)
    {
        if (!CanAccessEngineer(id)) return Forbid();

        var result = await _service.UpdateAsync(id, request);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPut("{id:int}/sales")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(EngineerDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignSales(int id, [FromBody] AssignSalesRequest request)
    {
        var result = await _service.AssignSalesAsync(id, request.PrimarySalesId);

        switch (result)
        {
            case AssignSalesResult.EngineerNotFound:
                return NotFound();
            case AssignSalesResult.SalesNotFound:
                return BadRequest(new
                {
                    error = new { code = "SALES_NOT_FOUND", message = "指定された担当営業が存在しません" }
                });
        }

        var detail = await _service.GetByIdAsync(id);
        return Ok(detail);
    }

    private int? ResolvePrimarySalesId(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;

        if (raw == "me")
        {
            var salesIdClaim = User.FindFirstValue("salesId");
            return int.TryParse(salesIdClaim, out var v) ? v : null;
        }

        return int.TryParse(raw, out var i) ? i : null;
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
