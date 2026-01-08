using System.Net;

namespace Application.Responses;

public class Response<T>
{
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }

    public Response()
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
    }

    public Response(T data)
    {
        Data = data;
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
        Message = null;
    }

    public Response(HttpStatusCode statusCode, string message)
    {
        Success = false;
        StatusCode = (int)statusCode;
        Message = message;
        Data = default;
    }
}