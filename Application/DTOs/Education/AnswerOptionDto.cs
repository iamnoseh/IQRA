namespace Application.DTOs.Education;

public class AnswerOptionDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MatchPairText { get; set; }
    public bool? IsCorrect { get; set; }
}
