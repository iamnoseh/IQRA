using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController(IOsonSmsService osonSmsService) : ControllerBase
{
    [HttpPost("test")]
    public async Task<IActionResult> TestSms([FromBody] TestSmsRequest request)
    {
        var result = await osonSmsService.SendSmsAsync(request.PhoneNumber, request.Message);
        
        if (result.Success)
            return Ok(new { message = "SMS муваффақият равон шуд", data = result.Data });
        
        return BadRequest(new { message = result.Message, statusCode = result.StatusCode });
    }

    [HttpGet("balance")]
    public async Task<IActionResult> CheckBalance()
    {
        var result = await osonSmsService.CheckBalanceAsync();
        
        if (result.Success)
            return Ok(new { message = "Баланс дарёфт шуд", data = result.Data });
        
        return BadRequest(new { message = result.Message, statusCode = result.StatusCode });
    }
}

public class TestSmsRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = "Test SMS аз IQRA";
}
