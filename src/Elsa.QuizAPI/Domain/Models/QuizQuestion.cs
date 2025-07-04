namespace Elsa.QuizAPI.Domain.Models;

public class QuizQuestion
{
    private QuizQuestion() { }

    public QuizQuestion(Guid quizId, string text, int points, QuizQuestionOption correctOption)
    {
        if (Guid.Empty.Equals(quizId))
            throw new ArgumentException("Quiz ID cannot be empty", nameof(quizId));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text cannot be empty", nameof(text));
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));
        if (correctOption is null || string.IsNullOrWhiteSpace(correctOption.OptionText) || !correctOption.IsCorrect)
            throw new ArgumentException("Invalid correct option", nameof(correctOption));

        QuestionId = Guid.NewGuid();
        QuizId = quizId;
        Text = text.Trim();
        Points = points;
        Options.Add(correctOption);
    }

    public Guid QuestionId { get; private set; }
    public Guid QuizId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public List<QuizQuestionOption> Options { get; private set; } = new();

    public void AddOption(string optionText, bool correctOption)
    {
        if (string.IsNullOrWhiteSpace(optionText))
            throw new ArgumentException("Option cannot be empty");
        if (Options.Any(o => o.OptionText.Equals(optionText, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Option already exists");

        Options.Add(new QuizQuestionOption(optionText, correctOption));
    }

    public void RemoveOption(string optionText)
    {
        var option = Options.SingleOrDefault(o => o.OptionText.Equals(optionText, StringComparison.OrdinalIgnoreCase));
        if (option is not null)
        {
            if (option.IsCorrect && Options.Where(o => o.IsCorrect).Count() == 1)
                throw new ArgumentException("Can not remove the correct answer");

            Options.Remove(option);
        }
    }

    public bool IsAnswerCorrect(string submittedAnswer)
    {
        if (string.IsNullOrWhiteSpace(submittedAnswer))
            return false;

        return Options.Any(o => o.IsCorrect && o.OptionText.Equals(submittedAnswer, StringComparison.OrdinalIgnoreCase));
    }
}

public class QuizQuestionOption
{
    public QuizQuestionOption(string optionText, bool isCorrect)
    {
        OptionText = optionText;
        IsCorrect = isCorrect;
    }

    public string OptionText { get; private set; }
    public bool IsCorrect { get; private set; }
}