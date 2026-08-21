using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class FilesController : ControllerBase
{
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Import([FromForm] IFormFile file)
    {
        return NoContent();
    }

    [HttpGet("latest-values")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetLatestValues([FromQuery] string filename)
    {
        return NoContent();
    }
}
