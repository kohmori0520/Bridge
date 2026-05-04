using System.Security.Claims;
using Bridge.Api.Dtos.Sales;
using Bridge.Api.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("sales")]
[Authorize(Policy = "SalesOnly")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _service;

    public SalesController(ISalesService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SalesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var result = await _service.ListAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SalesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(SalesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesRequest request)
    {
        // 認可:自分自身 or Admin のみ更新可
        if (!CanUpdateSales(id)) return Forbid();

        var result = await _service.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    private bool CanUpdateSales(int targetSalesId)
    {
        if (User.IsInRole("Admin")) return true;

        if (User.IsInRole("Sales"))
        {
            var mySalesIdClaim = User.FindFirstValue("salesId");
            return int.TryParse(mySalesIdClaim, out var mySalesId) && mySalesId == targetSalesId;
        }

        return false;
    }
}