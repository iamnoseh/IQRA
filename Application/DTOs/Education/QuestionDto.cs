namespace Application.DTOs.Education;

public class QuestionDto
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public string Difficulty { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    public List<AnswerOptionDto> Answers { get; set; } = new();
}
