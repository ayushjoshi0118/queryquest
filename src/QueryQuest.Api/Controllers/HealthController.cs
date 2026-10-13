using Microsoft.AspNetCore.Mvc;

namespace QueryQuest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "QueryQuest.Api",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
