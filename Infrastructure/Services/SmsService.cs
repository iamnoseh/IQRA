using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SmsService(IOsonSmsService osonSmsService, ILogger<SmsService> logger) : ISmsService
{
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var result = await osonSmsService.SendSmsAsync(phoneNumber, message);
            
            if (result.Success)
            {
                logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            else
            {
                logger.LogError("Failed to send SMS to {PhoneNumber}: {Message}", phoneNumber, result.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
