namespace Elsa.QuizAPI.Domain.Models;
public class Quiz
{
    private readonly List<QuizQuestion> _questions = new();
    
    private Quiz() { }

    public Quiz(string title, string description, TimeSpan timeLimit)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (timeLimit.TotalSeconds <= 0)
            throw new ArgumentException("Invalid time limit", nameof(timeLimit));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        TimeLimit = timeLimit;
    }

    public Guid QuizId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TimeSpan TimeLimit { get; private set; }
    
    public IReadOnlyList<QuizQuestion> Questions => _questions.AsReadOnly();
    
    // Computed properties
    public int TotalQuestions => _questions.Count;
    public int TotalPoints => _questions.Sum(q => q.Points);
    public bool HasQuestions => _questions.Any();

    public void AddQuestion(QuizQuestion question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));
        if (question.QuizId != QuizId)
            throw new ArgumentException("Question belongs to different quiz");
        if (_questions.Any(q => q.QuestionId == question.QuestionId))
            throw new ArgumentException("Question already exists in quiz");
            
        _questions.Add(question);
    }

    public QuizQuestion? GetQuestion(Guid questionId)
    {
        return _questions.FirstOrDefault(q => q.QuestionId == questionId);
    }

    public void RemoveQuestion(Guid questionId)
    {
        var question = _questions.FirstOrDefault(q => q.QuestionId == questionId);
        if (question != null)
        {
            _questions.Remove(question);
        }
    }

    public UserQuiz CreateUserQuiz(Guid userId)
    {
        return new UserQuiz(userId, this);
    }
}