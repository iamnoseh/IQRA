namespace Domain.Entities;

public class UserAnswer
{
    public long Id { get; set; }
    public Guid TestSessionId { get; set; }
    public long QuestionId { get; set; }
    
    public long? ChosenAnswerId { get; set; }
    public string? TextResponse { get; set; }
    
    public bool IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; }
    
    public TestSession TestSession { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public AnswerOption? ChosenAnswer { get; set; }
}
