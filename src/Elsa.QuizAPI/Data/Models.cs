namespace Elsa.QuizAPI.Data;

public class Quiz
{
    public string QuizId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan TimeLimit { get; set; }
    public ScoringType ScoringType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Question> Questions { get; set; } = new();

    // Cached lookup tables for performance
    // public Dictionary<string, string> AnswerKey { get; set; } = new();
    // public Dictionary<string, int> QuestionPoints { get; set; } = new();
}

public class Question
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class UserScore
{
    public string UserId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? CompletionTime { get; set; }
}

public class AnswerSubmission
{
    public string SubmissionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
}

public class SubmissionResult
{
    public string SubmissionId { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int NewTotalScore { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAlreadyProcessed { get; set; }
}

public class LeaderboardEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Rank { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? CompletionTime { get; set; }
}

public enum ScoringType
{
    Simple = 0,
    TimeBased = 1,
    StreakBased = 2
}

public enum QuestionType
{
    MultipleChoice = 0,
    TrueFalse = 1,
    Text = 2
}