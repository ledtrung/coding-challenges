using Elsa.QuizAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizAsync(string quizId);
    Task<Quiz?> GetQuizWithQuestionsAsync(string quizId, CancellationToken cancellationToken = default);
    Task<UserScore?> GetUserScoreAsync(string userId, string quizId);
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(string quizId, int limit = 10);
    Task<UserScore> CreateOrUpdateUserScoreAsync(UserScore userScore);
    Task<AnswerRecord> SaveAnswerRecordAsync(AnswerRecord answerRecord);
    Task<bool> IsAnswerAlreadySubmittedAsync(string submissionId);
    Task<AnswerRecord?> GetAnswerRecordAsync(string submissionId);
}

public class QuizRepository : IQuizRepository
{
    private readonly QuizDbContext _context;

    public QuizRepository(QuizDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizAsync(string quizId)
    {
        return await _context.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == quizId && q.IsActive);
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(string quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions.OrderBy(qu => qu.OrderIndex))
            .FirstOrDefaultAsync(q => q.QuizId == quizId && q.IsActive, cancellationToken);

        // if (quiz != null)
        // {
        //     // Build lookup tables for performance
        //     quiz.AnswerKey = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.CorrectAnswer);
        //     quiz.QuestionPoints = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.Points);
        // }

        return quiz;
    }

    public async Task<UserScore?> GetUserScoreAsync(string userId, string quizId)
    {
        return await _context.UserScores
            .FirstOrDefaultAsync(us => us.UserId == userId && us.QuizId == quizId);
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string quizId, int limit = 10)
    {
        return await _context.UserScores
            .Where(us => us.QuizId == quizId)
            .OrderByDescending(us => us.TotalScore)
            .ThenBy(us => us.CompletionTime)
            .Select((us, index) => new LeaderboardEntry
            {
                UserId = us.UserId,
                Username = us.Username,
                Score = us.TotalScore,
                Rank = index + 1,
                IsCompleted = us.IsCompleted,
                CompletionTime = us.CompletionTime
            })
            .Take(limit)
            .ToListAsync();
    }

    public async Task<UserScore> CreateOrUpdateUserScoreAsync(UserScore userScore)
    {
        var existing = await GetUserScoreAsync(userScore.UserId, userScore.QuizId);

        if (existing == null)
        {
            _context.UserScores.Add(userScore);
        }
        else
        {
            existing.TotalScore = userScore.TotalScore;
            existing.CorrectAnswers = userScore.CorrectAnswers;
            existing.TotalQuestions = userScore.TotalQuestions;
            existing.IsCompleted = userScore.IsCompleted;
            existing.CompletedAt = userScore.CompletedAt;
            existing.CompletionTime = userScore.CompletionTime;
        }

        await _context.SaveChangesAsync();
        return existing ?? userScore;
    }

    public async Task<AnswerRecord> SaveAnswerRecordAsync(AnswerRecord answerRecord)
    {
        _context.AnswerRecords.Add(answerRecord);
        await _context.SaveChangesAsync();
        return answerRecord;
    }

    public async Task<bool> IsAnswerAlreadySubmittedAsync(string submissionId)
    {
        return await _context.AnswerRecords
            .AnyAsync(ar => ar.SubmissionId == submissionId);
    }

    public async Task<AnswerRecord?> GetAnswerRecordAsync(string submissionId)
    {
        return await _context.AnswerRecords
            .FirstOrDefaultAsync(ar => ar.SubmissionId == submissionId);
    }
}