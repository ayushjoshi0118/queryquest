using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QueryQuest.Api.Data;
using QueryQuest.Api.Services;
using Xunit;

namespace QueryQuest.Tests;

public class SqlExecutionEngineTests : IDisposable
{
    private readonly string _tempDbPath;
    private readonly SqlExecutionEngine _engine;

    public SqlExecutionEngineTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"queryquest_test_{Guid.NewGuid():N}.db");
        SampleDatabaseSeeder.InitializeDatabase(_tempDbPath);
        _engine = new SqlExecutionEngine(_tempDbPath, NullLogger<SqlExecutionEngine>.Instance);
    }

    public void Dispose()
    {
        if (File.Exists(_tempDbPath))
        {
            try { File.Delete(_tempDbPath); } catch { }
        }
    }

    [Fact]
    public async Task ExecuteQuery_ValidSelect_ReturnsColumnsAndRows()
    {
        var result = await _engine.ExecuteQueryAsync("SELECT employee_id, first_name, salary FROM employees WHERE salary > 90000;");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(3, result.Columns.Count);
        Assert.Contains("employee_id", result.Columns);
        Assert.Contains("first_name", result.Columns);
        Assert.Contains("salary", result.Columns);
        Assert.True(result.RowCount > 0);
    }

    [Fact]
    public async Task ExecuteQuery_JoinQuery_ReturnsJoinedResults()
    {
        var result = await _engine.ExecuteQueryAsync(@"
            SELECT e.first_name, d.department_name 
            FROM employees e 
            JOIN departments d ON e.department_id = d.department_id;");

        Assert.True(result.Success);
        Assert.Equal(2, result.Columns.Count);
        Assert.True(result.RowCount >= 10);
    }

    [Fact]
    public async Task ExecuteQuery_AggregateAndGroupBy_ReturnsExpectedGroups()
    {
        var result = await _engine.ExecuteQueryAsync(@"
            SELECT department_id, COUNT(*) AS cnt 
            FROM employees 
            WHERE department_id IS NOT NULL 
            GROUP BY department_id 
            HAVING COUNT(*) >= 2;");

        Assert.True(result.Success);
        Assert.Equal(2, result.Columns.Count);
        Assert.True(result.RowCount > 0);
    }

    [Fact]
    public async Task ExecuteQuery_CommonTableExpression_ExecutesProperly()
    {
        var result = await _engine.ExecuteQueryAsync(@"
            WITH DeptCounts AS (
                SELECT department_id, COUNT(*) AS emp_count 
                FROM employees 
                GROUP BY department_id
            )
            SELECT * FROM DeptCounts;");

        Assert.True(result.Success);
        Assert.True(result.RowCount > 0);
    }

    [Fact]
    public async Task ExecuteQuery_WindowFunction_ExecutesProperly()
    {
        var result = await _engine.ExecuteQueryAsync(@"
            SELECT first_name, salary, DENSE_RANK() OVER (ORDER BY salary DESC) as rnk 
            FROM employees;");

        Assert.True(result.Success);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(12, result.RowCount);
    }

    [Fact]
    public async Task ExecuteQuery_EmptySql_ReturnsGracefulError()
    {
        var result = await _engine.ExecuteQueryAsync("   ");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ExecuteQuery_SyntaxError_ReturnsReadableErrorWithoutCrashing()
    {
        var result = await _engine.ExecuteQueryAsync("SELECT non_existent_column_xyz FROM non_existent_table_xyz;");

        Assert.False(result.Success);
        Assert.Contains("no such table", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("DROP TABLE employees;")]
    [InlineData("DELETE FROM employees;")]
    [InlineData("INSERT INTO employees (first_name, last_name) VALUES ('Hacker', 'Test');")]
    [InlineData("UPDATE employees SET salary = 999999;")]
    [InlineData("ALTER TABLE employees ADD COLUMN test_col TEXT;")]
    [InlineData("ATTACH DATABASE 'c:/evil.db' AS evil;")]
    [InlineData("PRAGMA foreign_keys = OFF;")]
    public async Task Security_MutationsAndDangerousCommands_AreRejected(string maliciousSql)
    {
        var result = await _engine.ExecuteQueryAsync(maliciousSql);

        Assert.False(result.Success);
        Assert.Contains("prohibited", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Security_MultipleStatements_AreRejected()
    {
        var result = await _engine.ExecuteQueryAsync("SELECT 1; SELECT 2;");

        Assert.False(result.Success);
        Assert.Contains("multiple SQL statements", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResultLimit_CappedAtMaxAllowedRows()
    {
        // Cartesian product query to generate over 200 rows
        var result = await _engine.ExecuteQueryAsync("SELECT e1.employee_id, e2.employee_id FROM employees e1 CROSS JOIN employees e2 CROSS JOIN employees e3;");

        Assert.True(result.Success);
        Assert.True(result.IsTruncated);
        Assert.Equal(200, result.RowCount);
    }

    [Fact]
    public async Task GetDatabaseSchemas_ReturnsAllCatalogTables()
    {
        var schemas = await _engine.GetDatabaseSchemasAsync();

        Assert.Equal(6, schemas.Count);
        Assert.Contains(schemas, s => s.TableName == "employees");
        Assert.Contains(schemas, s => s.TableName == "departments");
        Assert.Contains(schemas, s => s.TableName == "customers");
        Assert.Contains(schemas, s => s.TableName == "orders");
        Assert.Contains(schemas, s => s.TableName == "products");
        Assert.Contains(schemas, s => s.TableName == "order_items");
    }
}
