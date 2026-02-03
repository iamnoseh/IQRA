using Application.DTOs.Reference;
using Application.Responses;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("leaderboard")]
    public async Task<ActionResult<Response<List<SchoolLeaderboardDto>>>> GetLeaderboard()
    {
        var schools = await context.Schools
            .AsNoTracking()
            .OrderByDescending(s => s.TotalXP)
            .Take(20)
            .Select(s => new SchoolLeaderboardDto
            {
                Rank = 0,
                SchoolName = s.Name,
                District = s.District,
                TotalXP = s.TotalXP,
                StudentCount = s.StudentCount
            })
            .ToListAsync();
        for (int i = 0; i < schools.Count; i++)
        {
            schools[i].Rank = i + 1;
        }

        return Ok(new Response<List<SchoolLeaderboardDto>>(schools));
    }
}
