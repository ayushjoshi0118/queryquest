namespace QueryQuest.Api.Models;

public class Challenge
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    public int Order { get; set; }
    public string LearningObjective { get; set; } = string.Empty;
    public string ProblemStatement { get; set; } = string.Empty;
    public List<string> RequiredTables { get; set; } = new();
    public List<string> ExpectedColumns { get; set; } = new();
    public string StarterSql { get; set; } = string.Empty;
    public List<string> Hints { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public string ReferenceSolution { get; set; } = string.Empty;
    public bool RequiresExactOrdering { get; set; } = false;
    public int Points { get; set; } = 100;
}

public class UserProgress
{
    public int Id { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int AttemptsCount { get; set; }
    public int PointsAwarded { get; set; }
    public string? LastSubmittedSql { get; set; }
}

public class SubmissionRecord
{
    public int Id { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public string SubmittedSql { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? FeedbackMessage { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public long ExecutionTimeMs { get; set; }
}

public class AchievementBadge
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}

public class ExecuteQueryRequest
{
    public string Sql { get; set; } = string.Empty;
}

public class SubmitChallengeRequest
{
    public string Sql { get; set; } = string.Empty;
}

public class QueryResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<string> Columns { get; set; } = new();
    public List<List<object?>> Rows { get; set; } = new();
    public int RowCount { get; set; }
    public long ExecutionDurationMs { get; set; }
    public bool IsTruncated { get; set; }
}

public class SubmitResultDto
{
    public bool IsCorrect { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ConceptExplanation { get; set; }
    public QueryResultDto? UserResult { get; set; }
    public QueryResultDto? ExpectedResult { get; set; }
    public int PointsEarned { get; set; }
    public bool IsFirstCompletion { get; set; }
    public List<AchievementBadge> NewBadges { get; set; } = new();
}

public class HintResponseDto
{
    public int HintIndex { get; set; }
    public int TotalHints { get; set; }
    public string HintText { get; set; } = string.Empty;
    public bool HasMoreHints { get; set; }
}

public class TableSchemaDto
{
    public string TableName { get; set; } = string.Empty;
    public List<ColumnSchemaDto> Columns { get; set; } = new();
    public List<List<object?>> SampleRows { get; set; } = new();
}

public class ColumnSchemaDto
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
}

public class ProgressOverviewDto
{
    public int TotalChallenges { get; set; }
    public int CompletedChallenges { get; set; }
    public double CompletionPercentage { get; set; }
    public int TotalPoints { get; set; }
    public int TotalSubmissions { get; set; }
    public int SuccessfulSubmissions { get; set; }
    public double AccuracyRate { get; set; }
    public int CurrentStreakDays { get; set; }
    public List<CategoryProgressDto> Categories { get; set; } = new();
    public List<AchievementBadge> Badges { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class CategoryProgressDto
{
    public string Category { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
}

public class RecentActivityDto
{
    public string ChallengeId { get; set; } = string.Empty;
    public string ChallengeTitle { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime Timestamp { get; set; }
    public string SqlPreview { get; set; } = string.Empty;
}
