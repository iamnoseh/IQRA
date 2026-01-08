using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class LoginRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 7)]
    public string Password { get; set; } = string.Empty;
}
