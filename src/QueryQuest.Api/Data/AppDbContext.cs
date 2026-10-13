using Microsoft.EntityFrameworkCore;
using QueryQuest.Api.Models;

namespace QueryQuest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<SubmissionRecord> Submissions => Set<SubmissionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChallengeId).IsUnique();
            entity.Property(e => e.ChallengeId).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<SubmissionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChallengeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SubmittedSql).IsRequired();
            entity.HasIndex(e => e.SubmittedAt);
        });
    }
}
