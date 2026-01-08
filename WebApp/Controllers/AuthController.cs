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
    [HttpPost("send-password")]
    public async Task<IActionResult> SendPassword([FromBody] SendPasswordRequest request)
    {
        var result = await authService.SendPasswordAsync(request);
        
        if (string.IsNullOrEmpty(result.Token))
            return BadRequest(new { message = Messages.Auth.PasswordGenerationError });
        
        return Ok(new { message = Messages.Auth.PasswordSent, data = result });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        
        if (string.IsNullOrEmpty(result.Token))
            return BadRequest(new { message = Messages.Auth.UserAlreadyExists });
        
        return Ok(new { message = Messages.Auth.RegistrationSuccess, data = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        
        if (string.IsNullOrEmpty(result.Token))
            return Unauthorized(new { message = Messages.Auth.InvalidCredentials });
        
        return Ok(new { message = Messages.Auth.LoginSuccess, data = result });
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "Аутентификатсия кор мекунад!", userId = User.FindFirst("UserId")?.Value });
    }
}
