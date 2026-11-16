using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>Check service health, timestamp, and version.</summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "DocuMind.API",
            version = "1.0.0",
            timestamp = DateTimeOffset.UtcNow
        });
    }
}

