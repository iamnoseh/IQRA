namespace Application.DTOs.Testing;

public class TestSessionDto
{
    public Guid Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int TotalScore { get; set; }
    public string Mode { get; set; } = string.Empty;
    public List<Education.QuestionDto> Questions { get; set; } = new();
}
