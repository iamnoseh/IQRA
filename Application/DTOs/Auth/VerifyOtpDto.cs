using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class VerifyOtpDto
{
    [Required(ErrorMessage = "Номи корбар ҳатмист")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Рамзи тасдиқ ҳатмист")]
    public string OtpCode { get; set; } = string.Empty;
}
