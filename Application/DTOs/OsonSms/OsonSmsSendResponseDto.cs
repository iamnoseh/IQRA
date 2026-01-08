namespace Application.DTOs.OsonSms;

public class OsonSmsSendResponseDto
{
    public string? MsgId { get; set; }
    public OsonSmsError? Error { get; set; }
}

public class OsonSmsError
{
    public string Message { get; set; } = string.Empty;
    public int Code { get; set; }
}
