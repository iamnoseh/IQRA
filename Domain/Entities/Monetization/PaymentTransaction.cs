using Domain.Enums;

namespace Domain.Entities.Monetization;

public class PaymentTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public string? ExternalTransactionId { get; set; }
    
    public Users.AppUser User { get; set; } = null!;
}
