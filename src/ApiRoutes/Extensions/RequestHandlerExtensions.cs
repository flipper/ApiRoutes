using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace ApiRoutes;

[PublicAPI]
public static class RequestHandlerExtensions
{
    public static RequestResult Ok<T, TResponse>(this IHandler<T, TResponse> handler, TResponse value) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(TypedResults.Ok(value));
    }

    public static RequestResult NoContent<T>(this IHandler<T, None> handler) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(TypedResults.NoContent());
    }
    
    public static RequestResult Accepted<T, TResponse>(this IHandler<T, TResponse> handler, string? url = default, TResponse? value = default) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(TypedResults.Accepted(url, value));
    }

    public static RequestResult Error<T, TResponse>(this IHandler<T, TResponse> handler,
        RequestException exception) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(exception);
    }

    public static RequestResult Error<T>(this IHandler<T> handler,
        RequestException exception) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(exception);
    }
    
    public static RequestResult Error<T, TResponse>(this IHandler<T, TResponse> handler,
        HttpStatusCode statusCode, string message) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(new RequestException(statusCode, message));
    }
    
    public static RequestResult Error<T>(this IHandler<T> handler,
        HttpStatusCode statusCode, string message) where T : IBaseRequest<RequestResult>
    {
        return new RequestResult(new RequestException(statusCode, message));
    }
}