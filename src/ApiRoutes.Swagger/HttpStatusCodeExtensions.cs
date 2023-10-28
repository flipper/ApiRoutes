using System.Net;

namespace ApiRoutes.Swagger;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
    {
        var asInt = (int)statusCode;
        return asInt is >= 200 and <= 299;
    }
}