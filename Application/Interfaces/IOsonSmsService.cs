using Application.DTOs.OsonSms;
using Application.Responses;

namespace Application.Interfaces;

public interface IOsonSmsService
{
    Task<Response<OsonSmsSendResponseDto>> SendSmsAsync(string phoneNumber, string message);
    Task<Response<OsonSmsStatusResponseDto>> CheckSmsStatusAsync(string msgId);
    Task<Response<OsonSmsBalanceResponseDto>> CheckBalanceAsync();
}
