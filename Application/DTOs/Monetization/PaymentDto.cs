namespace Application.DTOs.Monetization;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
