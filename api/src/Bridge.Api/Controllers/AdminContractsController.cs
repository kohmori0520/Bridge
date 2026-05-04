using Bridge.Api.Dtos.ExpiringContracts;
using Bridge.Api.Services.ExpiringContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers;

[ApiController]
[Route("contracts")]
[Authorize(Policy = "AdminOnly")]
public class AdminContractsController : ControllerBase
{
    private readonly IExpiringContractService _service;

    public AdminContractsController(IExpiringContractService service)
    {
        _service = service;
    }

    [HttpGet("expiring")]
    [ProducesResponseType(typeof(ExpiringContractsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int days = 30)
    {
        days = Math.Clamp(days, 1, 365);

        // 全社対象なので filterBySalesId は null
        var result = await _service.GetAsync(days, filterBySalesId: null);
        return Ok(result);
    }
}