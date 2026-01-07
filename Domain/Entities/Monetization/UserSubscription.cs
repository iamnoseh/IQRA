namespace Domain.Entities.Monetization;

public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PlanId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    public Users.AppUser User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
