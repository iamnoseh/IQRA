namespace Application.DTOs.OsonSms;

public class OsonSmsBalanceResponseDto
{
    public decimal Balance { get; set; }
    public OsonSmsError? Error { get; set; }
}
