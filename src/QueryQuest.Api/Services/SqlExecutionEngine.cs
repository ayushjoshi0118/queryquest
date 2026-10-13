using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using QueryQuest.Api.Models;

namespace QueryQuest.Api.Services;

public interface ISqlExecutionEngine
{
    Task<QueryResultDto> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default);
    Task<List<TableSchemaDto>> GetDatabaseSchemasAsync(CancellationToken cancellationToken = default);
}

public class SqlExecutionEngine : ISqlExecutionEngine
{
    private readonly string _sampleDbPath;
    private readonly ILogger<SqlExecutionEngine> _logger;
    private const int MaxAllowedRows = 200;
    private const int ExecutionTimeoutSeconds = 4;

    // Reject dangerous statements, administrative commands, schema changes, and external file access
    private static readonly Regex DangerousCommandRegex = new(
        @"\b(ATTACH|DETACH|VACUUM|PRAGMA|ALTER|CREATE|DROP|INSERT|UPDATE|DELETE|REPLACE|UPSERT|TRUNCATE|LOAD_EXTENSION|WRITEFILE|READFILE)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SqlExecutionEngine(IConfiguration configuration, IWebHostEnvironment env, ILogger<SqlExecutionEngine> logger)
    {
        _logger = logger;
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        _sampleDbPath = Path.Combine(dataDir, "sample_educational.db");
    }

    // Constructor overload for tests/custom db path
    public SqlExecutionEngine(string dbPath, ILogger<SqlExecutionEngine> logger)
    {
        _sampleDbPath = dbPath;
        _logger = logger;
    }

    public async Task<QueryResultDto> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        var result = new QueryResultDto();
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(sql))
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = "Please enter a SQL query to execute.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        // 1. Clean query and check for disallowed patterns
        var trimmedSql = sql.Trim().TrimEnd(';');

        // Check for multiple semicolon-separated statements
        var statements = trimmedSql.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (statements.Length > 1)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = "Execution of multiple SQL statements is rejected for sandbox security.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        // Security check for data/schema mutation or sensitive sqlite keywords
        if (DangerousCommandRegex.IsMatch(trimmedSql))
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = "Write, schema modification, attachment, and administrative commands are prohibited in this educational playground.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        // Ensure the statement begins with read queries (SELECT, WITH, EXPLAIN)
        var cleanedWithoutComments = Regex.Replace(trimmedSql, @"(--[^\r\n]*)|(/\*[\s\S]*?\*/)", " ").Trim();
        if (!cleanedWithoutComments.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !cleanedWithoutComments.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) &&
            !cleanedWithoutComments.StartsWith("EXPLAIN", StringComparison.OrdinalIgnoreCase))
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = "Only read queries starting with SELECT or WITH (CTEs) are permitted.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        // Open read-only connection to isolated sample database
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = _sampleDbPath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared
        };

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ExecutionTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await using var connection = new SqliteConnection(connectionStringBuilder.ToString());
            await connection.OpenAsync(linkedCts.Token);

            await using var command = connection.CreateCommand();
            command.CommandText = trimmedSql;
            command.CommandTimeout = ExecutionTimeoutSeconds;

            await using var reader = await command.ExecuteReaderAsync(linkedCts.Token);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(reader.GetName(i));
            }

            var rows = new List<List<object?>>();
            var rowCount = 0;

            while (await reader.ReadAsync(linkedCts.Token))
            {
                if (rowCount >= MaxAllowedRows)
                {
                    result.IsTruncated = true;
                    break;
                }

                var row = new List<object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                    {
                        row.Add(null);
                    }
                    else
                    {
                        var val = reader.GetValue(i);
                        // Standardize numeric types to preserve precision and JSON serializability
                        if (val is double d)
                            row.Add(d);
                        else if (val is float f)
                            row.Add((double)f);
                        else if (val is long l)
                            row.Add(l);
                        else if (val is int n)
                            row.Add((long)n);
                        else
                            row.Add(val.ToString());
                    }
                }
                rows.Add(row);
                rowCount++;
            }

            stopwatch.Stop();
            result.Success = true;
            result.Rows = rows;
            result.RowCount = rows.Count;
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = $"Query execution exceeded the allowable time limit ({ExecutionTimeoutSeconds}s). Check for infinite loops or cartesian product joins.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
        }
        catch (SqliteException ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = SanitizeSqliteErrorMessage(ex.Message);
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected query execution failure");
            result.Success = false;
            result.Error = "An unexpected error occurred while executing the query.";
            result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<List<TableSchemaDto>> GetDatabaseSchemasAsync(CancellationToken cancellationToken = default)
    {
        var schemas = new List<TableSchemaDto>();
        var tables = new[] { "departments", "employees", "customers", "products", "orders", "order_items" };

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = _sampleDbPath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared
        };

        await using var connection = new SqliteConnection(connectionStringBuilder.ToString());
        await connection.OpenAsync(cancellationToken);

        foreach (var tableName in tables)
        {
            var tableSchema = new TableSchemaDto { TableName = tableName };

            // Fetch columns via PRAGMA table_info
            await using (var pragmaCmd = connection.CreateCommand())
            {
                pragmaCmd.CommandText = $"PRAGMA table_info({tableName});";
                await using var reader = await pragmaCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    tableSchema.Columns.Add(new ColumnSchemaDto
                    {
                        Name = reader.GetString(1),
                        DataType = reader.GetString(2),
                        IsNullable = reader.GetInt32(3) == 0,
                        IsPrimaryKey = reader.GetInt32(5) == 1
                    });
                }
            }

            // Fetch sample 5 rows
            await using (var sampleCmd = connection.CreateCommand())
            {
                sampleCmd.CommandText = $"SELECT * FROM {tableName} LIMIT 5;";
                await using var sampleReader = await sampleCmd.ExecuteReaderAsync(cancellationToken);
                while (await sampleReader.ReadAsync(cancellationToken))
                {
                    var row = new List<object?>();
                    for (var i = 0; i < sampleReader.FieldCount; i++)
                    {
                        row.Add(sampleReader.IsDBNull(i) ? null : sampleReader.GetValue(i));
                    }
                    tableSchema.SampleRows.Add(row);
                }
            }

            schemas.Add(tableSchema);
        }

        return schemas;
    }

    private static string SanitizeSqliteErrorMessage(string message)
    {
        // Strip out system paths or internal technical leakages
        var clean = Regex.Replace(message, @"(C:\\|/)[^\s:]+", "[path]");
        return clean;
    }
}
