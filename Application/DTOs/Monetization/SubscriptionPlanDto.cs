namespace Application.DTOs.Monetization;

public class SubscriptionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPremium { get; set; }
    public List<string> Features { get; set; } = new();
}
