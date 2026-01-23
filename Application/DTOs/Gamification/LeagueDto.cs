namespace Application.DTOs.Gamification;

public class LeagueDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinXP { get; set; }
    public string Color { get; set; } = string.Empty;
    public double PromotionThreshold { get; set; }
    public double RelegationThreshold { get; set; }
}

public class LeagueStandingDto
{
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int WeeklyXP { get; set; }
    public int Rank { get; set; }
    public string Trend { get; set; } = "STABLE"; // UP, DOWN, STABLE
    public bool IsCurrentUser { get; set; }
}
