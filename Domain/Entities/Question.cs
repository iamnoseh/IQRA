using Domain.Enums;

namespace Domain.Entities;

public class Question
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public int? TopicId { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public DifficultyLevel Difficulty { get; set; }
    public QuestionType Type { get; set; }
    
    public string Explanation { get; set; } = string.Empty;
    
    public Subject Subject { get; set; } = null!;
    public Topic? Topic { get; set; }
    public ICollection<AnswerOption> Answers { get; set; } = new List<AnswerOption>();
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
