namespace Application.DTOs.Testing;

public class AnswerFeedbackDto
{
    public bool IsCorrect { get; set; }
    public long? CorrectAnswerId { get; set; }
    public string? CorrectAnswerText { get; set; }
    public string FeedbackText { get; set; } = string.Empty;
    
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    public int? CorrectPairsCount { get; set; }
    public int? TotalPairsCount { get; set; }
}
