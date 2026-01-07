namespace Domain.Entities;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPremium { get; set; }
    
    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
