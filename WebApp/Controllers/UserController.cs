using System.Security.Claims;
using Application.DTOs.Users;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не определен" });

        var result = await userService.GetProfileAsync(userId);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Профиль найден", data = result.Data });
    }

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не определен" });

        var result = await userService.UpdateProfileAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, data = result.Data });
    }

    [HttpGet("profile/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfileByUsername(string username)
    {
        var result = await userService.GetProfileByUsernameAsync(username);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Профиль найден", data = result.Data });
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не определен" });

        var result = await userService.GetUserActivityAsync(userId);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Активность пользователя найдена", data = result.Data });
    }

    [HttpGet("test-activity")]
    public async Task<IActionResult> GetTestActivity([FromQuery] int days = 30)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не определен" });

        var result = await userService.GetTestActivityAsync(userId, days);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Статистика тестов найдена", data = result.Data });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications([FromServices] INotificationService notificationService)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не определен" });

        var result = await notificationService.GetUserNotificationsAsync(userId);
        return Ok(result);
    }

    [HttpPost("notifications/{id}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid id, [FromServices] INotificationService notificationService)
    {
        var result = await notificationService.MarkAsReadAsync(id);
        return Ok(result);
    }
}
