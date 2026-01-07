using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Monetization;

public class PurchaseSubscriptionRequest
{
    [Required]
    public int PlanId { get; set; }

    [Required]
    public string PaymentProvider { get; set; } = string.Empty;
}
