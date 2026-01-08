using System.Net;
using Application.Constants;
using Application.DTOs.OsonSms;
using Application.Responses;
using Application.Interfaces;
using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Infrastructure.Services;

public class OsonSmsService(IConfiguration configuration) : IOsonSmsService
{
    private readonly RestClient _restClient = new();
    private readonly string _login = configuration["OsonSmsSettings:Login"] ??
                                     throw new InvalidOperationException("OsonSmsSettings:Login not configured");
    private readonly string _passHash = configuration["OsonSmsSettings:PassHash"] ??
                                        throw new InvalidOperationException("OsonSmsSettings:PassHash not configured");
    private readonly string _sender = configuration["OsonSmsSettings:Sender"] ??
                                      throw new InvalidOperationException("OsonSmsSettings:Sender not configured");
    private readonly string _dlm = configuration["OsonSmsSettings:Dlm"] ??
                                   throw new InvalidOperationException("OsonSmsSettings:Dlm not configured");
    private readonly string _t = configuration["OsonSmsSettings:T"] ??
                                 throw new InvalidOperationException("OsonSmsSettings:T not configured");
    private readonly string _sendSmsUrl = configuration["OsonSmsSettings:SendSmsUrl"] ??
                                          throw new InvalidOperationException("OsonSmsSettings:SendSmsUrl not configured");
    private readonly string _checkSmsStatusUrl = configuration["OsonSmsSettings:CheckSmsStatusUrl"] ??
                                                 throw new InvalidOperationException("OsonSmsSettings:CheckSmsStatusUrl not configured");
    private readonly string _checkBalanceUrl = configuration["OsonSmsSettings:CheckBalanceUrl"] ??
                                               throw new InvalidOperationException("OsonSmsSettings:CheckBalanceUrl not configured");

    public async Task<Response<OsonSmsSendResponseDto>> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var txnId = HashHelper.GenerateTransactionId();
            var strHash = HashHelper.Sha256(txnId + _dlm + _login + _dlm + _sender + _dlm + phoneNumber + _dlm + _passHash);

            var request = new RestRequest(_sendSmsUrl);
            request.AddParameter("from", _sender);
            request.AddParameter("login", _login);
            request.AddParameter("t", _t);
            request.AddParameter("phone_number", phoneNumber);
            request.AddParameter("msg", message);
            request.AddParameter("str_hash", strHash);
            request.AddParameter("txn_id", txnId);

            var response = await _restClient.ExecuteAsync<OsonSmsSendResponseDto>(request);

            if (response is { IsSuccessful: true, Data: not null })
            {
                if (response.Data.Error != null)
                    return new Response<OsonSmsSendResponseDto>(HttpStatusCode.BadRequest, response.Data.Error.Message);

                return new Response<OsonSmsSendResponseDto>(response.Data) { Message = Messages.OsonSms.SendSuccess };
            }

            return new Response<OsonSmsSendResponseDto>(response.StatusCode, response.ErrorMessage ?? Messages.OsonSms.SendError);
        }
        catch (Exception ex)
        {
            return new Response<OsonSmsSendResponseDto>(HttpStatusCode.InternalServerError, string.Format(Messages.OsonSms.Error, ex.Message));
        }
    }

    public async Task<Response<OsonSmsStatusResponseDto>> CheckSmsStatusAsync(string msgId)
    {
        try
        {
            var txnId = HashHelper.GenerateTransactionId();
            var strHash = HashHelper.Sha256(_login + _dlm + txnId + _dlm + _passHash);

            var request = new RestRequest(_checkSmsStatusUrl);
            request.AddParameter("t", _t);
            request.AddParameter("login", _login);
            request.AddParameter("msg_id", msgId);
            request.AddParameter("str_hash", strHash);
            request.AddParameter("txn_id", txnId);

            var response = await _restClient.ExecuteAsync<OsonSmsStatusResponseDto>(request);

            if (response is { IsSuccessful: true, Data: not null })
            {
                if (response.Data.Error != null)
                    return new Response<OsonSmsStatusResponseDto>(HttpStatusCode.BadRequest, response.Data.Error.Message);

                return new Response<OsonSmsStatusResponseDto>(response.Data) { Message = Messages.OsonSms.StatusSuccess };
            }

            return new Response<OsonSmsStatusResponseDto>(response.StatusCode, response.ErrorMessage ?? Messages.OsonSms.StatusError);
        }
        catch (Exception ex)
        {
            return new Response<OsonSmsStatusResponseDto>(HttpStatusCode.InternalServerError, string.Format(Messages.OsonSms.Error, ex.Message));
        }
    }

    public async Task<Response<OsonSmsBalanceResponseDto>> CheckBalanceAsync()
    {
        try
        {
            var txnId = HashHelper.GenerateTransactionId();
            var strHash = HashHelper.Sha256(txnId + _dlm + _login + _dlm + _passHash);

            var request = new RestRequest(_checkBalanceUrl);
            request.AddParameter("t", _t);
            request.AddParameter("login", _login);
            request.AddParameter("str_hash", strHash);
            request.AddParameter("txn_id", txnId);

            var response = await _restClient.ExecuteAsync<OsonSmsBalanceResponseDto>(request);

            if (response is { IsSuccessful: true, Data: not null })
            {
                if (response.Data.Error != null)
                    return new Response<OsonSmsBalanceResponseDto>(HttpStatusCode.BadRequest, response.Data.Error.Message);

                return new Response<OsonSmsBalanceResponseDto>(response.Data) { Message = Messages.OsonSms.BalanceSuccess };
            }

            return new Response<OsonSmsBalanceResponseDto>(response.StatusCode, response.ErrorMessage ?? Messages.OsonSms.BalanceError);
        }
        catch (Exception ex)
        {
            return new Response<OsonSmsBalanceResponseDto>(HttpStatusCode.InternalServerError, string.Format(Messages.OsonSms.Error, ex.Message));
        }
    }
}
