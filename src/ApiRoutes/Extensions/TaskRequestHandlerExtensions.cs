using System.Net;
using JetBrains.Annotations;

namespace ApiRoutes;

[PublicAPI]
public static class TaskRequestHandlerExtensions
{
    public static ValueTask<RequestResult>
        OkAsTask<T, TResponse>(this IHandler<T, TResponse> handler, TResponse value)
        where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.Ok(value));
    }

    public static ValueTask<RequestResult>
        NoContentAsTask<T>(this IHandler<T, None> handler)
        where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.NoContent());
    }
    
    public static RequestResult AcceptedAsTask<T, TResponse>(this IHandler<T, TResponse> handler, string? url = default, TResponse? value = default) where T : IBaseRequest<RequestResult>
    {
        return handler.Accepted(url, value);
    }

    public static ValueTask<RequestResult> ErrorAsTask<T, TResponse>(
        this IHandler<T, TResponse> handler,
        RequestException exception) where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.Error(exception));
    }

    public static ValueTask<RequestResult> ErrorAsTask<T>(this IHandler<T> handler,
        RequestException exception) where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.Error(exception));
    }

    public static ValueTask<RequestResult> ErrorAsTask<T, TResponse>(
        this IHandler<T, TResponse> handler,
        HttpStatusCode statusCode, string message)
        where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.Error(statusCode, message));
    }

    public static ValueTask<RequestResult> ErrorAsTask<T>(this IHandler<T> handler,
        HttpStatusCode statusCode, string message) where T : IBaseRequest<RequestResult>
    {
        return ValueTask.FromResult(handler.Error(statusCode, message));
    }
}