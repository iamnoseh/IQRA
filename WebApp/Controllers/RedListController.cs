using Application.DTOs.Testing;
using Application.Interfaces;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RedListController(IRedListService redListService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Response<List<RedListQuestionDto>>>> GetRedList()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var result = await redListService.GetRedListAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("count")]
    public async Task<ActionResult<Response<int>>> GetRedListCount()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var result = await redListService.GetRedListCountAsync(userId.Value);
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<Response<RedListPracticeFeedbackDto>>> SubmitAnswer(SubmitRedListAnswerRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var result = await redListService.SubmitPracticeAnswerAsync(userId.Value, request);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return null;
        return Guid.Parse(userIdClaim);
    }
}
