using Microsoft.AspNetCore.Mvc;
using QueryQuest.Api.Models;
using QueryQuest.Api.Services;

namespace QueryQuest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly ISqlExecutionEngine _executionEngine;

    public QueryController(ISqlExecutionEngine executionEngine)
    {
        _executionEngine = executionEngine;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<QueryResultDto>> ExecuteQuery(
        [FromBody] ExecuteQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Sql))
        {
            return BadRequest(new QueryResultDto
            {
                Success = false,
                Error = "Query cannot be empty."
            });
        }

        if (request.Sql.Length > 4000)
        {
            return BadRequest(new QueryResultDto
            {
                Success = false,
                Error = "Query exceeds maximum allowed length of 4,000 characters."
            });
        }

        var result = await _executionEngine.ExecuteQueryAsync(request.Sql, cancellationToken);
        return Ok(result);
    }

    [HttpGet("schema")]
    public async Task<ActionResult<IEnumerable<TableSchemaDto>>> GetFullSchema(CancellationToken cancellationToken)
    {
        var schemas = await _executionEngine.GetDatabaseSchemasAsync(cancellationToken);
        return Ok(schemas);
    }
}
