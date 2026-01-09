namespace Application.DTOs.Testing;

public class QuestionWithAnswersDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<AnswerOptionDto> Answers { get; set; } = new();
}

public class AnswerOptionDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
