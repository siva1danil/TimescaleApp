using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ResultsController : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Search(
        [FromQuery] string? filename,
        [FromQuery] DateTimeOffset? startDateFrom,
        [FromQuery] DateTimeOffset? startDateTo,
        [FromQuery] double? averageValueFrom,
        [FromQuery] double? averageValueTo,
        [FromQuery] double? averageExecutionTimeFrom,
        [FromQuery] double? averageExecutionTimeTo)
    {
        return NoContent();
    }
}
