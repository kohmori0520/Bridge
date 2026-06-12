using System.Security.Claims;
using Bridge.Api.Dtos.Imports;
using Bridge.Api.Services.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Api.Controllers.Imports;

[ApiController]
[Route("imports")]
[Authorize(Policy = "SalesOnly")]
public class ImportsController : ControllerBase
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IExcelImportService _service;

    public ImportsController(IExcelImportService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTemplate()
    {
        var bytes = await _service.BuildTemplateAsync();
        return File(bytes, XlsxContentType, "bridge-import-template.xlsx");
    }

    [HttpPost("excel")]
    [RequestSizeLimit(MaxFileSizeBytes + 1024)]
    [ProducesResponseType(typeof(ImportResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportExcel(IFormFile? file, [FromQuery] bool dryRun = true)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ErrorBody("FILE_REQUIRED", "ファイルを指定してください"));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ErrorBody("FILE_TOO_LARGE", "ファイルサイズは 5MB 以下にしてください"));

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _service.ImportAsync(stream, dryRun, GetSalesIdFromClaims());
            return Ok(result);
        }
        catch (InvalidImportFileException ex)
        {
            return BadRequest(ErrorBody("INVALID_FILE", ex.Message));
        }
    }

    private int? GetSalesIdFromClaims()
    {
        var claim = User.FindFirstValue("salesId");
        return int.TryParse(claim, out var salesId) ? salesId : null;
    }

    private static object ErrorBody(string code, string message)
        => new { error = new { code, message } };
}
