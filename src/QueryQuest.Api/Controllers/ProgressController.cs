using Microsoft.AspNetCore.Mvc;
using QueryQuest.Api.Models;
using QueryQuest.Api.Services;

namespace QueryQuest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet]
    public async Task<ActionResult<ProgressOverviewDto>> GetProgress(CancellationToken cancellationToken)
    {
        var overview = await _progressService.GetProgressOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetActivity(CancellationToken cancellationToken)
    {
        var overview = await _progressService.GetProgressOverviewAsync(cancellationToken);
        return Ok(overview.RecentActivity);
    }

    [HttpPost("reset")]
    public async Task<ActionResult> ResetProgress(CancellationToken cancellationToken)
    {
        await _progressService.ResetProgressAsync(cancellationToken);
        return Ok(new { message = "Progress and submission records have been successfully reset." });
    }
}
