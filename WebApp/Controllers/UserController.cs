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
            return Unauthorized(new { message = "Корбар муайян карда нашуд" });

        var result = await userService.GetProfileAsync(userId);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Профил дарёфт шуд", data = result.Data });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Корбар муайян карда нашуд" });

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

        return Ok(new { message = "Профил дарёфт шуд", data = result.Data });
    }
}
