using Bridge.Api.Dtos.Skills;
using Bridge.Api.Services.Skills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("skills")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _service;

    public SkillsController(ISkillService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var result = await _service.ListAsync();
        return Ok(result);
    }
}