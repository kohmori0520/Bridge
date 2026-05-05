using System.Security.Claims;
using Bridge.Api.Dtos.Assignments;
using Bridge.Api.Services.Assignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Assignments;

[ApiController]
[Route("assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;

    public AssignmentsController(IAssignmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(AssignmentDetailResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request)
    {
        var result = await _service.CreateAsync(request);

        if (!result.Success)
        {
            var statusCode = result.ErrorCode switch
            {
                "DUPLICATE_ASSIGNMENT" => StatusCodes.Status409Conflict,
                "ENGINEER_NOT_FOUND" or "PROJECT_NOT_FOUND" => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest,
            };
            return StatusCode(statusCode, new { error = new { code = result.ErrorCode, message = result.ErrorMessage } });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Assignment!.Id }, result.Assignment);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AssignmentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound();
        if (!CanAccessAssignment(result)) return Forbid();
        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(AssignmentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAssignmentRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:int}/contracts")]
    [Authorize(Policy = "SalesOnly")]
    [ProducesResponseType(typeof(ContractItem), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddContract(int id, [FromBody] CreateContractRequest request)
    {
        var result = await _service.AddContractAsync(id, request);

        if (!result.Success)
        {
            var statusCode = result.ErrorCode switch
            {
                "PERIOD_OVERLAP" => StatusCodes.Status409Conflict,
                "ASSIGNMENT_NOT_FOUND" => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest,
            };
            return StatusCode(statusCode, new { error = new { code = result.ErrorCode, message = result.ErrorMessage } });
        }

        return CreatedAtAction(nameof(GetById), new { id }, result.Contract);
    }

    [HttpGet("{id:int}/contracts")]
    [ProducesResponseType(typeof(List<ContractItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContracts(int id)
    {
        var assignment = await _service.GetByIdAsync(id);
        if (assignment is null) return NotFound();
        if (!CanAccessAssignment(assignment)) return Forbid();

        var result = await _service.GetContractsByAssignmentIdAsync(id);
        return Ok(result);
    }

    private bool CanAccessAssignment(AssignmentDetailResponse assignment)
    {
        if (User.IsInRole("Sales") || User.IsInRole("Admin")) return true;

        if (User.IsInRole("Engineer"))
        {
            var myEngineerId = GetEngineerIdFromClaims();
            return myEngineerId == assignment.EngineerId;
        }

        return false;
    }

    private int? GetEngineerIdFromClaims()
    {
        var claim = User.FindFirstValue("engineerId");
        return int.TryParse(claim, out var v) ? v : null;
    }
}
