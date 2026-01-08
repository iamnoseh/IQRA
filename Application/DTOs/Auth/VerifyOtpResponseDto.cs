namespace Application.DTOs.Auth;

public class VerifyOtpResponseDto
{
    public string ResetToken { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
