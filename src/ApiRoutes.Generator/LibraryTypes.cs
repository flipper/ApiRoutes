namespace ApiRoutes.Generator;

public static class LibraryTypes
{
    public static readonly string HttpMethod = "global::ApiRoutes.Method";
    public static readonly string IHandlerWithoutResponse = "global::ApiRoutes.IHandler<TRequest>";
    public static readonly string IHandlerWithResponse = "global::ApiRoutes.IHandler<TRequest, TResponse>";
    public static readonly string RequestValidator = "global::ApiRoutes.RequestValidator<T>";
    public static readonly string RequestException = "global::ApiRoutes.RequestException";
    public static readonly string ApiRouteAttribute = "global::ApiRoutes.ApiRouteAttribute";
    public static readonly string FromFormAttribute = "global::Microsoft.AspNetCore.Mvc.FromFormAttribute";
    public static readonly string RequestBodyType = "global::ApiRoutes.RouteMetadata.RequestBodyType";
    
    public enum HttpMethodEnum
    {
        GET,
        POST,
        PUT,
        DELETE,
        CONNECT,
        OPTIONS,
        TRACE,
        PATCH
    }
}