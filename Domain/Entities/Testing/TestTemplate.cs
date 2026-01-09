namespace Domain.Entities.Testing;

public class TestTemplate
{
    public int Id { get; set; }
    public int ClusterNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SubjectDistributionJson { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int DurationMinutes { get; set; }
    
    public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
}
