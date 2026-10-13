using QueryQuest.Api.Data;
using QueryQuest.Api.Models;
using QueryQuest.Api.Services;
using Xunit;

namespace QueryQuest.Tests;

public class ChallengeValidatorTests
{
    private readonly ChallengeValidator _validator = new();

    [Fact]
    public void Validate_MatchingResults_ReturnsSuccess()
    {
        var challenge = new Challenge
        {
            Id = "test-1",
            Points = 100,
            RequiresExactOrdering = false,
            Explanation = "Test explanation"
        };

        var userResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id", "name" },
            Rows = new List<List<object?>>
            {
                new() { 1, "Alice" },
                new() { 2, "Bob" }
            },
            RowCount = 2
        };

        var expectedResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id", "name" },
            Rows = new List<List<object?>>
            {
                new() { 2, "Bob" },
                new() { 1, "Alice" }
            },
            RowCount = 2
        };

        var outcome = _validator.Validate(challenge, userResult, expectedResult);

        Assert.True(outcome.IsCorrect);
        Assert.Equal(100, outcome.PointsEarned);
    }

    [Fact]
    public void Validate_RequiresExactOrdering_FailsWhenOrderDiffers()
    {
        var challenge = new Challenge
        {
            Id = "test-order",
            Points = 100,
            RequiresExactOrdering = true,
            Explanation = "Order test"
        };

        var userResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id" },
            Rows = new List<List<object?>>
            {
                new() { 2 },
                new() { 1 }
            },
            RowCount = 2
        };

        var expectedResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id" },
            Rows = new List<List<object?>>
            {
                new() { 1 },
                new() { 2 }
            },
            RowCount = 2
        };

        var outcome = _validator.Validate(challenge, userResult, expectedResult);

        Assert.False(outcome.IsCorrect);
        Assert.Contains("ordering", outcome.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ColumnCountMismatch_ReturnsClearFeedback()
    {
        var challenge = new Challenge { Id = "test-col" };
        var userResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id" },
            Rows = new List<List<object?>> { new() { 1 } },
            RowCount = 1
        };

        var expectedResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "id", "name" },
            Rows = new List<List<object?>> { new() { 1, "Alice" } },
            RowCount = 1
        };

        var outcome = _validator.Validate(challenge, userResult, expectedResult);

        Assert.False(outcome.IsCorrect);
        Assert.Contains("Column count mismatch", outcome.Message);
    }

    [Fact]
    public void Validate_RowCountMismatch_ReturnsClearFeedback()
    {
        var challenge = new Challenge { Id = "test-row" };
        var userResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "name" },
            Rows = new List<List<object?>> { new() { "Alice" } },
            RowCount = 1
        };

        var expectedResult = new QueryResultDto
        {
            Success = true,
            Columns = new List<string> { "name" },
            Rows = new List<List<object?>> { new() { "Alice" }, new() { "Bob" } },
            RowCount = 2
        };

        var outcome = _validator.Validate(challenge, userResult, expectedResult);

        Assert.False(outcome.IsCorrect);
        Assert.Contains("Row count mismatch", outcome.Message);
    }
}
