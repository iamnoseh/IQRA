using Application.DTOs.CMS;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotivationController(IMotivationService motivationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MotivationalQuoteDto>>> GetAll()
    {
        return Ok(await motivationService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MotivationalQuoteDto>> GetById(int id)
    {
        var quote = await motivationService.GetByIdAsync(id);
        if (quote == null) return NotFound();
        return Ok(quote);
    }

    [HttpGet("random")]
    public async Task<ActionResult<MotivationalQuoteDto>> GetRandom()
    {
        return Ok(await motivationService.GetRandomQuoteAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MotivationalQuoteDto>> Create(CreateMotivationalQuoteDto dto)
    {
        var created = await motivationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, MotivationalQuoteDto dto)
    {
        if (id != dto.Id) return BadRequest();
        await motivationService.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await motivationService.DeleteAsync(id);
        return NoContent();
    }
}
