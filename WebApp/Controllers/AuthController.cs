using Application.Constants;
using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        
        if (string.IsNullOrEmpty(result.Token))
            return BadRequest(new { message = "Хатогӣ ҳангоми бақайдгирӣ" });
        
        return Ok(new { message = "Бақайдгирӣ муваффақ. Парол ба рақам равон карда шуд", data = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        
        if (string.IsNullOrEmpty(result.Token))
            return Unauthorized(new { message = Messages.Auth.InvalidCredentials });
        
        return Ok(new { message = Messages.Auth.LoginSuccess, data = result });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var message = await authService.ChangePasswordAsync(changePasswordDto);
        
        if (message != Messages.Auth.PasswordChanged)
            return BadRequest(new { message });
        
        return Ok(new { message });
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto sendOtpDto)
    {
        var message = await authService.SendOtpAsync(sendOtpDto);
        
        if (message != Messages.Auth.OtpSent)
            return BadRequest(new { message });
        
        return Ok(new { message });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
    {
        var result = await authService.VerifyOtpAsync(verifyOtpDto);
        
        if (string.IsNullOrEmpty(result.ResetToken))
            return BadRequest(new { message = result.Message });
        
        return Ok(new { message = result.Message, data = result });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var message = await authService.ResetPasswordAsync(resetPasswordDto);
        
        if (message != Messages.Auth.PasswordReset)
            return BadRequest(new { message });
        
        return Ok(new { message });
    }


}
