namespace Domain.Entities.Gamification;

public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinXP { get; set; }
    
    public string Color { get; set; } = "#CD7F32";
    public string IconUrl { get; set; } = string.Empty;
    public double PromotionThreshold { get; set; } = 0.20;
    public double RelegationThreshold { get; set; } = 0.20;
    
    public ICollection<Users.UserProfile> Users { get; set; } = new List<Users.UserProfile>();
    public int MaxXP { get; set; }
    public string BadgeUrl { get; set; } = string.Empty;
}
