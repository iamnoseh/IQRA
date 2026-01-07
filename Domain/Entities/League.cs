namespace Domain.Entities;

public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinXP { get; set; }
    public int MaxXP { get; set; }
    public string BadgeUrl { get; set; } = string.Empty;
}
