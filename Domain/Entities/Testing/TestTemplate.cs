namespace Domain.Entities.Testing;

public class TestTemplate
{
    public int Id { get; set; }
    public int ClusterId { get; set; }
    public Domain.Enums.ComponentType ComponentType { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public int SingleChoiceCount { get; set; } = 10;
    public int ClosedAnswerCount { get; set; } = 5;
    public int MatchingCount { get; set; } = 5;
    
    public int DurationMinutes { get; set; } = 180;
    public bool IsActive { get; set; } = true;
    
    public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
}
