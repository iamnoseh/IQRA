using Domain.Enums;

namespace Application.DTOs.Testing;

public class TestSessionDto
{
    public Guid Id { get; set; }
    public TestMode Mode { get; set; }
    public int ClusterNumber { get; set; }
    public int TotalScore { get; set; }
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
