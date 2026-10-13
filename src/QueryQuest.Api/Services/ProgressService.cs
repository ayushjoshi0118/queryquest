using Microsoft.EntityFrameworkCore;
using QueryQuest.Api.Data;
using QueryQuest.Api.Models;

namespace QueryQuest.Api.Services;

public interface IProgressService
{
    Task<ProgressOverviewDto> GetProgressOverviewAsync(CancellationToken cancellationToken = default);
    Task<SubmitResultDto> RecordSubmissionAsync(string challengeId, string submittedSql, SubmitResultDto validationResult, CancellationToken cancellationToken = default);
    Task ResetProgressAsync(CancellationToken cancellationToken = default);
    Task<UserProgress?> GetChallengeProgressAsync(string challengeId, CancellationToken cancellationToken = default);
}

public class ProgressService : IProgressService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(AppDbContext dbContext, ILogger<ProgressService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserProgress?> GetChallengeProgressAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ChallengeId == challengeId, cancellationToken);
    }

    public async Task<ProgressOverviewDto> GetProgressOverviewAsync(CancellationToken cancellationToken = default)
    {
        var allChallenges = ChallengeCatalog.GetAll();
        var userProgressList = await _dbContext.UserProgresses.AsNoTracking().ToListAsync(cancellationToken);
        var submissions = await _dbContext.Submissions.AsNoTracking().ToListAsync(cancellationToken);

        var completedCount = userProgressList.Count(p => p.IsCompleted);
        var totalPoints = userProgressList.Where(p => p.IsCompleted).Sum(p => p.PointsAwarded);
        var totalSubs = submissions.Count;
        var successfulSubs = submissions.Count(s => s.IsCorrect);
        var accuracy = totalSubs > 0 ? Math.Round((double)successfulSubs / totalSubs * 100.0, 1) : 0.0;
        var completionPct = allChallenges.Count > 0 ? Math.Round((double)completedCount / allChallenges.Count * 100.0, 1) : 0.0;

        // Categories breakdown
        var categories = allChallenges
            .GroupBy(c => c.Category)
            .Select(g => new CategoryProgressDto
            {
                Category = g.Key,
                Total = g.Count(),
                Completed = g.Count(c => userProgressList.Any(p => p.ChallengeId == c.Id && p.IsCompleted))
            })
            .ToList();

        // Calculate badges
        var badges = ComputeBadges(allChallenges, userProgressList, submissions);

        // Calculate streak
        var streakDays = CalculateStreakDays(submissions);

        // Recent activity
        var challengeLookup = allChallenges.ToDictionary(c => c.Id);
        var recentActivity = submissions
            .OrderByDescending(s => s.SubmittedAt)
            .Take(8)
            .Select(s => new RecentActivityDto
            {
                ChallengeId = s.ChallengeId,
                ChallengeTitle = challengeLookup.TryGetValue(s.ChallengeId, out var ch) ? ch.Title : s.ChallengeId,
                IsCorrect = s.IsCorrect,
                Timestamp = s.SubmittedAt,
                SqlPreview = s.SubmittedSql.Length > 60 ? s.SubmittedSql[..57] + "..." : s.SubmittedSql
            })
            .ToList();

        return new ProgressOverviewDto
        {
            TotalChallenges = allChallenges.Count,
            CompletedChallenges = completedCount,
            CompletionPercentage = completionPct,
            TotalPoints = totalPoints,
            TotalSubmissions = totalSubs,
            SuccessfulSubmissions = successfulSubs,
            AccuracyRate = accuracy,
            CurrentStreakDays = streakDays,
            Categories = categories,
            Badges = badges,
            RecentActivity = recentActivity
        };
    }

    public async Task<SubmitResultDto> RecordSubmissionAsync(string challengeId, string submittedSql, SubmitResultDto validationResult, CancellationToken cancellationToken = default)
    {
        var challenge = ChallengeCatalog.GetById(challengeId);
        if (challenge == null) return validationResult;

        // 1. Record the submission attempt
        var submission = new SubmissionRecord
        {
            ChallengeId = challengeId,
            SubmittedSql = submittedSql,
            IsCorrect = validationResult.IsCorrect,
            FeedbackMessage = validationResult.Message,
            SubmittedAt = DateTime.UtcNow,
            ExecutionTimeMs = validationResult.UserResult?.ExecutionDurationMs ?? 0
        };
        _dbContext.Submissions.Add(submission);

        // 2. Lookup or create UserProgress
        var progress = await _dbContext.UserProgresses.FirstOrDefaultAsync(p => p.ChallengeId == challengeId, cancellationToken);
        if (progress == null)
        {
            progress = new UserProgress
            {
                ChallengeId = challengeId,
                AttemptsCount = 1,
                LastSubmittedSql = submittedSql
            };
            _dbContext.UserProgresses.Add(progress);
        }
        else
        {
            progress.AttemptsCount += 1;
            progress.LastSubmittedSql = submittedSql;
        }

        // 3. Prevent duplicate rewards: award points only on the FIRST completion
        if (validationResult.IsCorrect)
        {
            if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.PointsAwarded = challenge.Points;
                validationResult.IsFirstCompletion = true;
                validationResult.PointsEarned = challenge.Points;
            }
            else
            {
                // Already completed previously, 0 new points
                validationResult.IsFirstCompletion = false;
                validationResult.PointsEarned = 0;
            }
        }
        else
        {
            validationResult.PointsEarned = 0;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Check for new badges
        var overview = await GetProgressOverviewAsync(cancellationToken);
        validationResult.NewBadges = overview.Badges.Where(b => b.IsUnlocked).ToList();

        return validationResult;
    }

    public async Task ResetProgressAsync(CancellationToken cancellationToken = default)
    {
        _dbContext.Submissions.RemoveRange(_dbContext.Submissions);
        _dbContext.UserProgresses.RemoveRange(_dbContext.UserProgresses);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<AchievementBadge> ComputeBadges(IReadOnlyList<Challenge> challenges, List<UserProgress> progressList, List<SubmissionRecord> submissions)
    {
        var completedSet = progressList.Where(p => p.IsCompleted).Select(p => p.ChallengeId).ToHashSet();
        var completedCount = completedSet.Count;

        var badges = new List<AchievementBadge>
        {
            new AchievementBadge
            {
                Code = "FIRST_QUERY",
                Title = "First Query",
                Description = "Successfully solve your first SQL challenge.",
                Icon = "compass",
                IsUnlocked = completedCount >= 1,
                UnlockedAt = progressList.Where(p => p.IsCompleted).OrderBy(p => p.CompletedAt).FirstOrDefault()?.CompletedAt
            },
            new AchievementBadge
            {
                Code = "FILTER_MASTER",
                Title = "Filter Master",
                Description = "Complete all challenges in Filtering and Sorting.",
                Icon = "filter",
                IsUnlocked = challenges.Where(c => c.Category == "Filtering and Sorting").All(c => completedSet.Contains(c.Id)),
                UnlockedAt = progressList.Where(p => p.IsCompleted && p.ChallengeId.StartsWith("c8-")).FirstOrDefault()?.CompletedAt
            },
            new AchievementBadge
            {
                Code = "JOIN_EXPLORER",
                Title = "Join Explorer",
                Description = "Master relationships and complete all Joins challenges.",
                Icon = "link-2",
                IsUnlocked = challenges.Where(c => c.Category == "Joins").All(c => completedSet.Contains(c.Id)),
                UnlockedAt = progressList.Where(p => p.IsCompleted && p.ChallengeId.StartsWith("c16-")).FirstOrDefault()?.CompletedAt
            },
            new AchievementBadge
            {
                Code = "AGGREGATION_EXPERT",
                Title = "Aggregation Expert",
                Description = "Successfully complete all Aggregation challenges.",
                Icon = "bar-chart-2",
                IsUnlocked = challenges.Where(c => c.Category == "Aggregation").All(c => completedSet.Contains(c.Id)),
                UnlockedAt = progressList.Where(p => p.IsCompleted && p.ChallengeId.StartsWith("c12-")).FirstOrDefault()?.CompletedAt
            },
            new AchievementBadge
            {
                Code = "SQL_ADVENTURER",
                Title = "SQL Adventurer",
                Description = "Complete 12 or more challenges across QueryQuest.",
                Icon = "award",
                IsUnlocked = completedCount >= 12,
                UnlockedAt = progressList.Where(p => p.IsCompleted).OrderBy(p => p.CompletedAt).Skip(11).FirstOrDefault()?.CompletedAt
            },
            new AchievementBadge
            {
                Code = "MASTER_ARCHITECT",
                Title = "Master Query Architect",
                Description = "Conquer all 24 curriculum challenges with flying colors.",
                Icon = "zap",
                IsUnlocked = completedCount >= challenges.Count && challenges.Count > 0,
                UnlockedAt = progressList.Where(p => p.IsCompleted).OrderBy(p => p.CompletedAt).LastOrDefault()?.CompletedAt
            }
        };

        return badges;
    }

    private static int CalculateStreakDays(List<SubmissionRecord> submissions)
    {
        if (!submissions.Any(s => s.IsCorrect)) return 0;

        var dates = submissions
            .Where(s => s.IsCorrect)
            .Select(s => s.SubmittedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var today = DateTime.UtcNow.Date;
        var streak = 0;
        var current = today;

        // If today has no submission, check if yesterday had one to maintain active streak
        if (!dates.Contains(today))
        {
            if (dates.Contains(today.AddDays(-1)))
            {
                current = today.AddDays(-1);
            }
            else
            {
                return 0;
            }
        }

        while (dates.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }

        return streak;
    }
}
