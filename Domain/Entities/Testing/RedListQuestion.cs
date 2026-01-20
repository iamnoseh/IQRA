using Domain.Entities.Users;
using Domain.Entities.Education;

namespace Domain.Entities.Testing;

public class RedListQuestion
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public long QuestionId { get; set; }
    
    public int ConsecutiveCorrectCount { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastPracticedAt { get; set; }
    
    public AppUser User { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
