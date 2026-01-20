namespace Application.DTOs.Testing;

public class QuestionWithAnswersDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public Domain.Enums.DifficultyLevel Difficulty { get; set; }
    public Domain.Enums.QuestionType Type { get; set; }
    public List<AnswerOptionDto> Answers { get; set; } = new();
    public List<string> MatchOptions { get; set; } = new();
    
    // Red List Info
    public bool IsInRedList { get; set; }
    public int RedListCorrectCount { get; set; }
}

public class AnswerOptionDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MatchPairText { get; set; }
}
