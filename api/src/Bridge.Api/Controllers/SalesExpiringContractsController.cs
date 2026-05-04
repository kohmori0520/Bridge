using System.Security.Claims;
using Bridge.Api.Dtos.ExpiringContracts;
using Bridge.Api.Services.ExpiringContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("sales/me")]
[Authorize(Policy = "SalesOnly")]
public class SalesExpiringContractsController : ControllerBase
{
    private readonly IExpiringContractService _service;

    public SalesExpiringContractsController(IExpiringContractService service)
    {
        _service = service;
    }

    [HttpGet("expiring-contracts")]
    [ProducesResponseType(typeof(ExpiringContractsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int days = 30)
    {
        var salesIdClaim = User.FindFirstValue("salesId");
        if (!int.TryParse(salesIdClaim, out var salesId))
            return Forbid();

        // クエリ防御:days は 1〜365 の範囲に制限
        days = Math.Clamp(days, 1, 365);

        var result = await _service.GetAsync(days, filterBySalesId: salesId);
        return Ok(result);
    }
}