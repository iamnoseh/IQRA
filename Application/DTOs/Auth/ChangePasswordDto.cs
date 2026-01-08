using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class ChangePasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
