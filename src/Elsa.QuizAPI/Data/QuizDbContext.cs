using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Elsa.QuizAPI.Data;

public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<UserScore> UserScores { get; set; }
    public DbSet<AnswerRecord> AnswerRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Quiz configuration
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId);
            // entity.Property(e => e.AnswerKey)
            //       .HasConversion(
            //           v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //           v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new());
            // entity.Property(e => e.QuestionPoints)
            //       .HasConversion(
            //           v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //           v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null) ?? new());
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId);
        });

        // UserScore configuration
        modelBuilder.Entity<UserScore>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QuizId });
        });

        // AnswerRecord configuration
        modelBuilder.Entity<AnswerRecord>(entity =>
        {
            entity.HasKey(e => e.SubmissionId);
        });
    }
}


public class AnswerRecord
{
    public string SubmissionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string SubmittedAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public DateTime SubmittedAt { get; set; }
    public TimeSpan ResponseTime { get; set; }
}