using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class SendOtpDto
{
    [Required(ErrorMessage = "Номи корбар ҳатмист")]
    public string Username { get; set; } = string.Empty;
}
