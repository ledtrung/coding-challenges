using Elsa.QuizAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace Elsa.QuizAPI.Features.Quizzes;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizService quizService, ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    [HttpPost("submit-answer")]
    public async Task<ActionResult<SubmissionResult>> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        try
        {
            var submission = new AnswerSubmission
            {
                SubmissionId = request.SubmissionId ?? Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Username = request.Username,
                QuizId = request.QuizId,
                QuestionId = request.QuestionId,
                Answer = request.Answer.Trim(),
                Timestamp = DateTime.UtcNow,
                ResponseTime = TimeSpan.FromMilliseconds(request.ResponseTimeMs)
            };

            // Process answer and publish events to Redis
            var result = await _quizService.SubmitAnswerAsync(submission);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit answer");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{quizId}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(string quizId, CancellationToken cancellationToken)
    {
        try
        {
            // Quiz API handles all business logic
            var quiz = await _quizService.GetQuizAsync(quizId, cancellationToken);
            if (quiz == null) return NotFound();

            return Ok(new QuizDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimit = quiz.TimeLimit,
                TotalQuestions = quiz.Questions.Count
            });
        }
        catch (OperationCanceledException)
        {
            return new EmptyResult();
        }
    }

    // [HttpGet("{quizId}/questions")]
    // public async Task<ActionResult<List<QuestionDto>>> GetQuizQuestions(string quizId)
    // {
    //     var questions = await _quizService.GetQuizQuestionsAsync(quizId);
    //     return Ok(questions);
    // }

    // [HttpGet("user/{userId}/quiz/{quizId}/state")]
    // public async Task<UserQuizState> GetUserQuizState(string userId, string quizId)
    // {
    //     return new UserQuizState
    //     {
    //         Quiz = await _cachedQuizService.GetQuizAsync(quizId),
    //         UserScore = await _quizRepository.GetUserScoreAsync(userId, quizId),
    //         AnsweredQuestions = await _quizRepository.GetUserAnswerHistoryAsync(userId, quizId),
    //         Leaderboard = await _quizRepository.GetLeaderboardAsync(quizId),
    //         LastSyncTimestamp = DateTime.UtcNow
    //     };
    // }
}

public class QuizDto
{
    public string QuizId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan TimeLimit { get; set; }
    public int TotalQuestions { get; set; }
    public string ScoringType { get; set; } = string.Empty;
}

public class QuestionDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Points { get; set; }
    public int OrderIndex { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class SubmitAnswerRequest
{
    public string? SubmissionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int ResponseTimeMs { get; set; }
}

public class UserQuizState
{
    public string UserId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public Quiz Quiz { get; set; } = new();
    public UserScore? UserScore { get; set; }
    public Dictionary<string, AnswerStateDto> AnsweredQuestions { get; set; } = new();
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();
    public DateTime LastSyncTimestamp { get; set; }
    public bool IsQuizCompleted { get; set; }
}

public class AnswerStateDto
{
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string SubmissionId { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}