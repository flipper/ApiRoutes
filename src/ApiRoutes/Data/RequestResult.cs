using Microsoft.AspNetCore.Http;

namespace ApiRoutes;

public readonly struct RequestResult
{
    private readonly bool _isSuccess;
    public readonly IResult Result;
    public readonly RequestException Error;

    public bool IsSuccess => _isSuccess;

    public bool IsError => !_isSuccess;
    
    public RequestResult(IResult result)
    {
        _isSuccess = true;
        Error = default!;
        Result = result;
    }

    public RequestResult(RequestException error)
    {
        _isSuccess = false;
        Result = default!;
        Error = error;
    }
}