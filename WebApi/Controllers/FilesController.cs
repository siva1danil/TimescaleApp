using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;
using Services.Interfaces.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class FilesController(
    IFileImportService fileImportService,
    IValueService valueService) : ControllerBase
{
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportAsync(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        var filename = Path.GetFileName(file.FileName);

        await fileImportService.ImportAsync(filename, content, cancellationToken);

        return NoContent();
    }

    [HttpGet("latest-values")]
    [ProducesResponseType(typeof(IReadOnlyList<ValueModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ValueModel>>> GetLatestValuesAsync(
        [FromQuery] string filename,
        CancellationToken cancellationToken)
    {
        var values = await valueService.GetLatestAsync(filename, cancellationToken);

        return Ok(values);
    }
}
