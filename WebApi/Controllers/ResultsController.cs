using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;
using Services.Interfaces.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ResultsController(IResultService resultService) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ResultModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResultModel>>> SearchAsync(
        [FromQuery] string? filename,
        [FromQuery] DateTimeOffset? startDateFrom,
        [FromQuery] DateTimeOffset? startDateTo,
        [FromQuery] double? averageValueFrom,
        [FromQuery] double? averageValueTo,
        [FromQuery] double? averageExecutionTimeFrom,
        [FromQuery] double? averageExecutionTimeTo,
        CancellationToken cancellationToken)
    {
        var filter = new ResultSearchFilter
        {
            Filename = filename,
            FirstOperationDateFrom = startDateFrom,
            FirstOperationDateTo = startDateTo,
            AverageValueFrom = averageValueFrom,
            AverageValueTo = averageValueTo,
            AverageExecutionTimeFrom = averageExecutionTimeFrom,
            AverageExecutionTimeTo = averageExecutionTimeTo
        };

        var results = await resultService.SearchAsync(filter, cancellationToken);

        return Ok(results);
    }
}
