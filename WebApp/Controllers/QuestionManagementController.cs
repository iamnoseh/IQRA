using Application.DTOs.Testing.Management;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/questions/manage")]
[Authorize(Roles = "Admin")]
public class QuestionManagementController(IQuestionManagementService questionService) : ControllerBase
{
    [HttpPost("questions/import")]
    public async Task<IActionResult> ImportQuestions([FromBody] BulkQuestionImportRequest request)
    {
        var result = await questionService.ImportQuestionsAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        var summary = $"{result.Data!.SuccessCount}/{result.Data.TotalQuestions} муваффақ";
        if (result.Data.FailedCount > 0)
            summary += $", {result.Data.FailedCount} хатогӣ";

        return Ok(new { 
            message = summary,
            data = result.Data 
        });
    }

    [HttpPost("questions/validate")]
    public async Task<IActionResult> ValidateImport([FromBody] BulkQuestionImportRequest request)
    {
        request.ValidateOnly = true;
        var result = await questionService.ImportQuestionsAsync(request);
        
        return Ok(new { 
            message = "Санҷиш тамом",
            data = result.Data 
        });
    }

    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion([FromForm] CreateQuestionRequest request)
    {
        var result = await questionService.CreateQuestionAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Савол эҷод шуд", questionId = result.Data!.Id });
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(long id, [FromBody] QuestionImportDto dto)
    {
        var result = await questionService.UpdateQuestionAsync(id, dto);
        
        if (!result.Success)
            return result.StatusCode == (int)System.Net.HttpStatusCode.NotFound 
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = "Савол навсозӣ шуд" });
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(long id)
    {
        var result = await questionService.DeleteQuestionAsync(id);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Савол нест карда шуд" });
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAllQuestions([FromQuery] QuestionFilterRequest filter)
    {
        var result = await questionService.GetAllQuestionsAsync(filter);
        return Ok(result.Data);
    }

    [HttpGet("questions/subject/{subjectId}")]
    public async Task<IActionResult> GetQuestionsBySubject(int subjectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await questionService.GetQuestionsBySubjectAsync(subjectId, page, pageSize);
        
        return Ok(new { 
            message = $"Саволҳои фан {subjectId}",
            data = result.Data 
        });
    }

    [HttpGet("questions/{id}")]
    public async Task<IActionResult> GetQuestion(long id)
    {
        var result = await questionService.GetQuestionByIdAsync(id);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });
            
        return Ok(new { message = "Савол", data = result.Data });
    }

    [HttpGet("questions/stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await questionService.GetQuestionStatsAsync();
        return Ok(new { message = "Оморҳо", data = result.Data });
    }
}
