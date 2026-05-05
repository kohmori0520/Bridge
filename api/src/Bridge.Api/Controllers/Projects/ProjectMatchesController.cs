using Bridge.Api.Dtos.Matching;
using Bridge.Api.Services.Matching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Projects;

[ApiController]
[Route("projects")]
[Authorize(Policy = "SalesOnly")]
public class ProjectMatchesController : ControllerBase
{
    private readonly IMatchingService _service;

    public ProjectMatchesController(IMatchingService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}/matches")]
    [ProducesResponseType(typeof(ProjectMatchesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatches(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(page, 1);
        limit = Math.Clamp(limit, 1, 100);

        var result = await _service.GetMatchesForProjectAsync(id, page, limit);
        return result is null ? NotFound() : Ok(result);
    }
}
