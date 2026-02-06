using System.Security.Claims;
using Application.DTOs.Reference;
using Application.Interfaces;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolsController(ISchoolService schoolService) : ControllerBase
{
    [HttpGet("leaderboard")]
    [Authorize]
    public async Task<ActionResult<Response<SchoolLeaderboardResponse>>> GetLeaderboard()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;

        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await schoolService.GetLeaderboardAsync(userId);
        return Ok(result);
    }
}
