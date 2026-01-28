using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamificationController : ControllerBase
{
    [HttpGet("badges")]
    public IActionResult GetBadges()
    {
        var badges = new List<object>
        {
            new { Code = "first_test", Name = "Аввалин қадам", IsUnlocked = true, Description = "Аввалин тестро супоридед", IconUrl = "/icons/badge1.png" },
            new { Code = "winner", Name = "Ғолиб", IsUnlocked = false, Description = "Дар 5 дуэл ғолиб шавед", IconUrl = "/icons/badge2.png", Progress = "2/5" },
            new { Code = "streak_3", Name = "Се рӯзи фаъол", IsUnlocked = true, Description = "3 рӯз пайиҳам ба барнома даромадед", IconUrl = "/icons/badge3.png" }
        };

        return Ok(new Response<List<object>>(badges));
    }

    [HttpGet("profile-stats")]
    public async Task<IActionResult> GetStats([FromServices] Infrastructure.Data.ApplicationDbContext context)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound("Profile not found");

        return Ok(new { profile.XP, profile.WeeklyXP, profile.EloRating });
    }
}
