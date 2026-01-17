using Domain.Enums;

namespace Domain.Entities.Education;

public class Question
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public string? Topic { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public DifficultyLevel Difficulty { get; set; }
    public QuestionType Type { get; set; }
    
    public string Explanation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Subject Subject { get; set; } = null!;
    public ICollection<AnswerOption> Answers { get; set; } = new List<AnswerOption>();
    public ICollection<Testing.UserAnswer> UserAnswers { get; set; } = new List<Testing.UserAnswer>();
}
