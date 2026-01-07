namespace Application.DTOs.Testing;

public class TestResultDto
{
    public Guid TestSessionId { get; set; }
    public int TotalScore { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public bool IsPassed { get; set; }
    public int XPEarned { get; set; }
    public List<QuestionResultDto> Results { get; set; } = new();
}

public class QuestionResultDto
{
    public long QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public long? UserAnswerId { get; set; }
    public long CorrectAnswerId { get; set; }
    public string? Explanation { get; set; }
}
