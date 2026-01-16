using Application.DTOs.Reference;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectController(ISubjectService subjectService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllForSelect()
    {
        var result = await subjectService.GetAllForSelectAsync();
        return Ok(new { message = "Рӯйхати фанҳо", data = result.Data });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await subjectService.GetByIdAsync(id);
        
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = "Фан", data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateSubjectRequest request)
    {
        var result = await subjectService.CreateSubjectAsync(request);
        
        if (!result.Success)
            return result.StatusCode == (int)System.Net.HttpStatusCode.Conflict 
                ? Conflict(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, data = result.Data });
    }
}
