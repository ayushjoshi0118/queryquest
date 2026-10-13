using Microsoft.AspNetCore.Mvc;
using QueryQuest.Api.Data;
using QueryQuest.Api.Models;
using QueryQuest.Api.Services;

namespace QueryQuest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly ISqlExecutionEngine _executionEngine;
    private readonly IChallengeValidator _validator;
    private readonly IProgressService _progressService;
    private readonly ILogger<ChallengesController> _logger;

    public ChallengesController(
        ISqlExecutionEngine executionEngine,
        IChallengeValidator validator,
        IProgressService progressService,
        ILogger<ChallengesController> logger)
    {
        _executionEngine = executionEngine;
        _validator = validator;
        _progressService = progressService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllChallenges(CancellationToken cancellationToken)
    {
        var challenges = ChallengeCatalog.GetAll();
        var overview = await _progressService.GetProgressOverviewAsync(cancellationToken);
        var completedMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Fetch completed IDs
        var userProgressList = await _progressService.GetProgressOverviewAsync(cancellationToken);
        // Map completions directly from DB
        var result = new List<object>();
        foreach (var ch in challenges)
        {
            var progress = await _progressService.GetChallengeProgressAsync(ch.Id, cancellationToken);
            result.Add(new
            {
                ch.Id,
                ch.Title,
                ch.Category,
                ch.Difficulty,
                ch.Order,
                ch.Points,
                ch.LearningObjective,
                ch.RequiredTables,
                IsCompleted = progress?.IsCompleted ?? false,
                AttemptsCount = progress?.AttemptsCount ?? 0
            });
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetChallengeById(string id, CancellationToken cancellationToken)
    {
        var challenge = ChallengeCatalog.GetById(id);
        if (challenge == null)
        {
            return NotFound(new { error = $"Challenge with ID '{id}' was not found." });
        }

        var progress = await _progressService.GetChallengeProgressAsync(id, cancellationToken);

        return Ok(new
        {
            challenge.Id,
            challenge.Title,
            challenge.Category,
            challenge.Difficulty,
            challenge.Order,
            challenge.LearningObjective,
            challenge.ProblemStatement,
            challenge.RequiredTables,
            challenge.ExpectedColumns,
            challenge.StarterSql,
            challenge.RequiresExactOrdering,
            challenge.Points,
            IsCompleted = progress?.IsCompleted ?? false,
            LastSubmittedSql = progress?.LastSubmittedSql,
            AttemptsCount = progress?.AttemptsCount ?? 0,
            TotalHintsAvailable = challenge.Hints.Count
        });
    }

    [HttpGet("{id}/schema")]
    public async Task<ActionResult<IEnumerable<TableSchemaDto>>> GetChallengeSchema(string id, CancellationToken cancellationToken)
    {
        var challenge = ChallengeCatalog.GetById(id);
        if (challenge == null)
        {
            return NotFound(new { error = $"Challenge with ID '{id}' was not found." });
        }

        var allSchemas = await _executionEngine.GetDatabaseSchemasAsync(cancellationToken);
        var filtered = allSchemas
            .Where(s => challenge.RequiredTables.Contains(s.TableName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return Ok(filtered.Any() ? filtered : allSchemas);
    }

    [HttpPost("{id}/hint")]
    public ActionResult<HintResponseDto> GetNextHint(string id, [FromQuery] int hintIndex = 0)
    {
        var challenge = ChallengeCatalog.GetById(id);
        if (challenge == null)
        {
            return NotFound(new { error = $"Challenge with ID '{id}' was not found." });
        }

        if (challenge.Hints.Count == 0)
        {
            return Ok(new HintResponseDto
            {
                HintIndex = 0,
                TotalHints = 0,
                HintText = "No hints are configured for this challenge.",
                HasMoreHints = false
            });
        }

        var safeIndex = Math.Clamp(hintIndex, 0, challenge.Hints.Count - 1);
        return Ok(new HintResponseDto
        {
            HintIndex = safeIndex,
            TotalHints = challenge.Hints.Count,
            HintText = challenge.Hints[safeIndex],
            HasMoreHints = safeIndex + 1 < challenge.Hints.Count
        });
    }

    [HttpGet("{id}/solution")]
    public ActionResult<object> GetReferenceSolution(string id)
    {
        var challenge = ChallengeCatalog.GetById(id);
        if (challenge == null)
        {
            return NotFound(new { error = $"Challenge with ID '{id}' was not found." });
        }

        return Ok(new
        {
            challenge.Id,
            challenge.ReferenceSolution,
            challenge.Explanation
        });
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<SubmitResultDto>> SubmitChallengeSolution(
        string id,
        [FromBody] SubmitChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var challenge = ChallengeCatalog.GetById(id);
        if (challenge == null)
        {
            return NotFound(new { error = $"Challenge with ID '{id}' was not found." });
        }

        if (string.IsNullOrWhiteSpace(request?.Sql))
        {
            return BadRequest(new { error = "Submission SQL cannot be empty." });
        }

        if (request.Sql.Length > 4000)
        {
            return BadRequest(new { error = "Submission SQL query exceeds the 4,000 character limit." });
        }

        // 1. Execute user query
        var userResult = await _executionEngine.ExecuteQueryAsync(request.Sql, cancellationToken);

        // 2. Execute canonical reference query to get exact expected output
        var expectedResult = await _executionEngine.ExecuteQueryAsync(challenge.ReferenceSolution, cancellationToken);

        // 3. Result-based validation
        var validation = _validator.Validate(challenge, userResult, expectedResult);

        // 4. Record submission & persist progress
        var finalResult = await _progressService.RecordSubmissionAsync(challenge.Id, request.Sql, validation, cancellationToken);

        return Ok(finalResult);
    }
}
