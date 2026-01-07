using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class LoginRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string SmsCode { get; set; } = string.Empty;
}
