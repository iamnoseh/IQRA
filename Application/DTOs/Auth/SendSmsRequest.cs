using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class SendSmsRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}
