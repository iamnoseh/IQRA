using Microsoft.AspNetCore.Identity;
using Domain.Enums;

namespace Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
    
    public UserProfile? Profile { get; set; }
    public UserSubscription? Subscription { get; set; }
    public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    public ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
    public ICollection<DuelMatch> DuelsAsPlayer1 { get; set; } = new List<DuelMatch>();
    public ICollection<DuelMatch> DuelsAsPlayer2 { get; set; } = new List<DuelMatch>();
}
