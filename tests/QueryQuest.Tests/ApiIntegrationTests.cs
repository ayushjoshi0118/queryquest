using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QueryQuest.Api.Data;
using QueryQuest.Api.Models;
using System.Net.Http.Json;
using Xunit;

namespace QueryQuest.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task Challenges_GetAll_ReturnsAtLeast24Challenges()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/challenges");

        response.EnsureSuccessStatusCode();
        var challenges = await response.Content.ReadFromJsonAsync<List<ChallengeSummaryDto>>();
        Assert.NotNull(challenges);
        Assert.True(challenges.Count >= 24);
    }

    [Fact]
    public async Task Challenges_GetById_ReturnsCorrectDetails()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/challenges/c1-select-all-employees");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Select All Employees", content);
        Assert.Contains("employees", content);
    }

    [Fact]
    public async Task QueryExecute_ReturnsRealSqlResults()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/query/execute", new ExecuteQueryRequest
        {
            Sql = "SELECT customer_id, first_name, city FROM customers ORDER BY customer_id LIMIT 3;"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QueryResultDto>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(3, result.RowCount);
        Assert.Equal(3, result.Columns.Count);
    }

    [Fact]
    public async Task ChallengeSubmit_CorrectSolution_AwardsPointsAndRecordsProgress()
    {
        var client = _factory.CreateClient();

        // 1. Reset progress first to guarantee clean slate
        await client.PostAsync("/api/progress/reset", null);

        // 2. Submit correct solution
        var response = await client.PostAsJsonAsync("/api/challenges/c1-select-all-employees/submit", new SubmitChallengeRequest
        {
            Sql = "SELECT * FROM employees;"
        });

        response.EnsureSuccessStatusCode();
        var submitResult = await response.Content.ReadFromJsonAsync<SubmitResultDto>();
        Assert.NotNull(submitResult);
        Assert.True(submitResult.IsCorrect);
        Assert.True(submitResult.IsFirstCompletion);
        Assert.Equal(100, submitResult.PointsEarned);

        // 3. Verify progress endpoint reflects completion
        var progressResp = await client.GetAsync("/api/progress");
        progressResp.EnsureSuccessStatusCode();
        var progress = await progressResp.Content.ReadFromJsonAsync<ProgressOverviewDto>();
        Assert.NotNull(progress);
        Assert.True(progress.CompletedChallenges >= 1);
        Assert.True(progress.TotalPoints >= 100);

        // 4. Repeated submission should not award duplicate points
        var repeatResponse = await client.PostAsJsonAsync("/api/challenges/c1-select-all-employees/submit", new SubmitChallengeRequest
        {
            Sql = "SELECT * FROM employees;"
        });
        var repeatResult = await repeatResponse.Content.ReadFromJsonAsync<SubmitResultDto>();
        Assert.NotNull(repeatResult);
        Assert.True(repeatResult.IsCorrect);
        Assert.False(repeatResult.IsFirstCompletion);
        Assert.Equal(0, repeatResult.PointsEarned);
    }

    [Fact]
    public async Task ChallengeSubmit_IncorrectSolution_ReturnsHelpfulErrorFeedback()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/challenges/c1-select-all-employees/submit", new SubmitChallengeRequest
        {
            Sql = "SELECT employee_id FROM employees WHERE salary > 999999;"
        });

        response.EnsureSuccessStatusCode();
        var submitResult = await response.Content.ReadFromJsonAsync<SubmitResultDto>();
        Assert.NotNull(submitResult);
        Assert.False(submitResult.IsCorrect);
        Assert.Contains("mismatch", submitResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    private class ChallengeSummaryDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
