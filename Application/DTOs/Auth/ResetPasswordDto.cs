using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Token ҳатмист")]
    public string ResetToken { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Рамзи нав ҳатмист")]
    [MinLength(6, ErrorMessage = "Рамз набояд аз 6 рамз кам бошад")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Тасдиқи рамз ҳатмист")]
    [Compare("NewPassword", ErrorMessage = "Рамзҳо мувофиқат намекунанд")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
