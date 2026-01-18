using Application.DTOs.Testing;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController(ITestService testService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> StartTest([FromBody] StartTestRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var result = await testService.StartTestAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Тест оғоз шуд", testSessionId = result.Data });
    }

    [HttpGet("{testSessionId}/questions")]
    public async Task<IActionResult> GetQuestions(Guid testSessionId)
    {
        var result = await testService.GetTestQuestionsAsync(testSessionId);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Саволҳо", data = result.Data });
    }

    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var result = await testService.SubmitAnswerAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Ҷавоб қабул шуд", data = result.Data });
    }

    [HttpPost("{testSessionId}/finish")]
    public async Task<IActionResult> FinishTest(Guid testSessionId)
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var result = await testService.FinishTestAsync(userId, testSessionId);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Тест тамом шуд", data = result.Data });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var result = await testService.GetUserTestHistoryAsync(userId, page, pageSize);
        
        return Ok(new { message = "Истории тестҳо", data = result.Data });
    }
}
