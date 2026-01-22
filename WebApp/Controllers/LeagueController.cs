using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeagueController(ILeagueService leagueService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLeagues()
    {
        var result = await leagueService.GetLeaguesAsync();
        return Ok(result);
    }

    [HttpGet("{leagueId}/standings")]
    public async Task<IActionResult> GetStandings(int leagueId)
    {
        var userIdString = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await leagueService.GetStandingsAsync(userId, leagueId);
        return Ok(result);
    }
}
