using Application.Interfaces;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
}
