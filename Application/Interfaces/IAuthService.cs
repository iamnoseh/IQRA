using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginDto loginDto);
    Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
    Task<string> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<string> SendOtpAsync(SendOtpDto sendOtpDto);
    Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpDto verifyOtpDto);
    Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
