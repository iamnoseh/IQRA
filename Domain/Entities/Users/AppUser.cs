using Microsoft.AspNetCore.Identity;
using Domain.Enums;

namespace Domain.Entities.Users;

public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
    
    public UserProfile? Profile { get; set; }
    public Monetization.UserSubscription? Subscription { get; set; }
    public ICollection<Testing.TestSession> TestSessions { get; set; } = new List<Testing.TestSession>();
    public ICollection<Monetization.PaymentTransaction> Payments { get; set; } = new List<Monetization.PaymentTransaction>();
    public ICollection<Gamification.DuelMatch> DuelsAsPlayer1 { get; set; } = new List<Gamification.DuelMatch>();
    public ICollection<Gamification.DuelMatch> DuelsAsPlayer2 { get; set; } = new List<Gamification.DuelMatch>();
}
