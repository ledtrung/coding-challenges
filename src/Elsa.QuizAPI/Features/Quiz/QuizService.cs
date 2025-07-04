
using Elsa.QuizAPI.Data;
using Elsa.QuizAPI.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Elsa.QuizAPI.Features.Quizzes;

public interface IQuizService
{
    Task<Quiz?> GetQuizAsync(string quizId, CancellationToken cancellationToken = default);
    Task<SubmissionResult> SubmitAnswerAsync(AnswerSubmission submission);
    Task<UserScore?> GetUserScoreAsync(string userId, string quizId);
}

public class QuizService : IQuizService
{
    private readonly HybridCache _cache;
    private readonly IQuizRepository _quizRepository;
    private readonly IScoreCalculator _scoreCalculator;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<QuizService> _logger;

    public QuizService(HybridCache cache, IQuizRepository quizRepository, IScoreCalculator scoreCalculator, IEventPublisher eventPublisher, ILogger<QuizService> logger)
    {
        _cache = cache;
        _quizRepository = quizRepository;
        _scoreCalculator = scoreCalculator;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Quiz?> GetQuizAsync(string quizId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"quiz:{quizId}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                _logger.LogInformation($"Quiz {quizId} loaded from database and cached");
                return await _quizRepository.GetQuizWithQuestionsAsync(quizId, cancellationToken);
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(30)
            },
            cancellationToken: cancellationToken);
    }
    
    public async Task<SubmissionResult> SubmitAnswerAsync(AnswerSubmission submission)
    {
        try
        {
            // 1. Process answer (business logic)
            var quiz = await GetQuizAsync(submission.QuizId);
            var question = quiz.Questions.FirstOrDefault(q => q.QuestionId == submission.QuestionId);

            // 2. Calculate score
            var userScore = await _quizRepository.GetUserScoreAsync(submission.UserId, submission.QuizId) ?? new UserScore();
            var pointsEarned = await _scoreCalculator.CalculateScoreAsync(quiz, question, submission, userScore);
            var isCorrect = pointsEarned > 0;

            // 3. Update database
            userScore.TotalScore += pointsEarned;
            userScore.TotalQuestions++;
            if (isCorrect) userScore.CorrectAnswers++;

            await _quizRepository.CreateOrUpdateUserScoreAsync(userScore);

            // // 4. Publish events to Redis (WebSocket service will consume these)
            // await _eventPublisher.PublishScoreUpdateAsync(new ScoreUpdateEvent
            // {
            //     UserId = submission.UserId,
            //     Username = submission.Username,
            //     QuizId = submission.QuizId,
            //     NewScore = userScore.TotalScore,
            //     PointsEarned = pointsEarned,
            //     IsCorrect = isCorrect,
            //     QuestionId = submission.QuestionId
            // });

            // // 5. Update leaderboard
            // var leaderboard = await _quizRepository.GetLeaderboardAsync(submission.QuizId);
            // await _eventPublisher.PublishLeaderboardUpdateAsync(new LeaderboardUpdateEvent
            // {
            //     QuizId = submission.QuizId,
            //     Leaderboard = leaderboard
            // });

            return new SubmissionResult
            {
                SubmissionId = submission.SubmissionId,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                NewTotalScore = userScore.TotalScore,
                CorrectAnswer = question.CorrectAnswer,
                Message = isCorrect ? "Correct!" : "Incorrect"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process answer submission");
            throw;
        }
    }

    public async Task<UserScore?> GetUserScoreAsync(string userId, string quizId)
    {
        return await _quizRepository.GetUserScoreAsync(userId, quizId);
    }
}