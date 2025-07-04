namespace Elsa.QuizAPI.Domain.Models;
public class User
{
    private readonly List<UserQuiz> _quizAttempts = new();
    
    private User() { }
    
    public User(Guid userId, string username)
    {
        if (Guid.Empty.Equals(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
            
        UserId = userId;
        Username = username.Trim();
    }

    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    
    public IReadOnlyList<UserQuiz> QuizAttempts => _quizAttempts.AsReadOnly();

    public UserQuiz JoinQuiz(Quiz quiz)
    {
        var userQuiz = new UserQuiz(UserId, quiz);
        _quizAttempts.Add(userQuiz);

        return userQuiz;
    }

    public UserQuiz? GetQuizAttempt(Guid userQuizId)
    {
        return _quizAttempts.FirstOrDefault(ua => ua.UserQuizId == userQuizId);
    }

    public UserQuiz? GetActiveQuizAttempt(Guid quizId)
    {
        return _quizAttempts.FirstOrDefault(ua => ua.QuizId == quizId && ua.IsInProgress);
    }
}