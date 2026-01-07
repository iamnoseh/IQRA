namespace Domain.Entities;

public class AnswerOption
{
    public long Id { get; set; }
    public long QuestionId { get; set; }
    
    public string Text { get; set; } = string.Empty;
    public string? MatchPairText { get; set; }
    public bool IsCorrect { get; set; }
    
    public Question Question { get; set; } = null!;
}
