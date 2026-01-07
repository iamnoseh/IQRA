using Domain.Enums;

namespace Domain.Entities;

public class TestSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    
    public int TotalScore { get; set; }
    public TestMode Mode { get; set; }
    
    public AppUser User { get; set; } = null!;
    public ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
}
