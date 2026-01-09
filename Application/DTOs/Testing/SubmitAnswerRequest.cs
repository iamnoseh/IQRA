namespace Application.DTOs.Testing;

public class SubmitAnswerRequest
{
    public Guid TestSessionId { get; set; }
    public long QuestionId { get; set; }
    public long ChosenAnswerId { get; set; }
    public int TimeSpentSeconds { get; set; }
}
