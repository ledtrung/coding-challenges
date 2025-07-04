namespace Elsa.QuizAPI.Domain.Models;

public class UserQuizQuestion
{
    private UserQuizQuestion() { }
    
    public UserQuizQuestion(Guid userQuizId, QuizQuestion question)
    {
        if (Guid.Empty.Equals(userQuizId))
            throw new ArgumentException("User quiz ID cannot be empty", nameof(userQuizId));
        if (Guid.Empty.Equals(question.QuestionId))
            throw new ArgumentNullException("Question ID cannot be empty", nameof(question));
            
        UserQuizQuestionId = Guid.NewGuid();
        UserQuizId = userQuizId;
        QuestionId = question.QuestionId;
    }

    public Guid UserQuizQuestionId { get; private set; }
    public Guid UserQuizId { get; private set; }
    public Guid QuestionId { get; private set; }
    
    // Answer details
    public bool IsAnswered { get; private set; }
    public string? SubmittedAnswer { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public bool IsCorrect { get; private set; }
    public int PointsEarned { get; private set; }

    public void SubmitAnswer(string answer, QuizQuestion question)
    {
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Answer cannot be empty", nameof(answer));
            
        SubmittedAnswer = answer.Trim();
        AnsweredAt = DateTime.UtcNow;
        IsAnswered = true;
        IsCorrect = question.IsAnswerCorrect(SubmittedAnswer);
        
        if (IsCorrect)
        {
            PointsEarned = question.Points;
        }
        else
        {
            PointsEarned = 0;
        }
    }
}