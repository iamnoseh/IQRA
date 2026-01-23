using Application.Interfaces;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.DTOs.Dashboard;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IAiService aiService) : ControllerBase
{
    [HttpGet("motivation")]
    public async Task<ActionResult<Response<string>>> GetMotivation()
    {
        var motivation = await aiService.GetDashboardMotivationAsync();
        return Ok(new Response<string>(motivation));
    }

    [HttpGet("student-stats")]
    public IActionResult GetStudentStats()
    {
        var stats = new DashboardStatsDto
        {
            DailyActivity = Enumerable.Range(0, 7).Select(i => new DailyActivityDto
            {
                Date = DateTime.Now.AddDays(-6 + i).ToString("yyyy-MM-dd"),
                TestsCount = new Random().Next(0, 10)
            }).ToList(),

            SubjectPerformance = new List<SubjectPerformanceDto>
            {
                new() { Subject = "Математика", Score = 78 },
                new() { Subject = "Физика", Score = 65 },
                new() { Subject = "Англисӣ", Score = 85 },
                new() { Subject = "Таърих", Score = 50 }
            },

            TodoRedListCount = 12,

            UniversityProbability = new List<UniversityProbabilityDto>
            {
                new() { Name = "ДМТ", Percent = 75 },
                new() { Name = "ДТТ", Percent = 60 },
                new() { Name = "ДДМТ", Percent = 40 }
            }
        };

        return Ok(new Response<DashboardStatsDto>(stats));
    }
}
