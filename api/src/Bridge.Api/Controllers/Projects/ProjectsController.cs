using System.Security.Claims;
using Bridge.Api.Dtos.Projects;
using Bridge.Api.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Projects;

[ApiController]
[Route("projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProjectListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? ownerSalesId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var resolvedOwnerId = ResolveOwnerSalesId(ownerSalesId);

        var query = new ProjectListQuery
        {
            OwnerSalesId = resolvedOwnerId,
            Status = status,
            Page = page,
            Limit = limit,
        };

        var result = await _service.ListAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var ownerSalesId = GetSalesIdFromClaims();
        if (ownerSalesId is null) return Forbid();

        var result = await _service.CreateAsync(request, ownerSalesId.Value);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.SoftDeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    private int? ResolveOwnerSalesId(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;

        if (raw == "me")
            return GetSalesIdFromClaims();

        return int.TryParse(raw, out var i) ? i : null;
    }

    private int? GetSalesIdFromClaims()
    {
        var claim = User.FindFirstValue("salesId");
        return int.TryParse(claim, out var v) ? v : null;
    }
}
