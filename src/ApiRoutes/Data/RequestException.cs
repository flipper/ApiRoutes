using System.Net;
using System.Runtime.Serialization;

namespace ApiRoutes;

[Serializable]
public class RequestException : Exception
{
    private readonly HttpStatusCode _statusCode;

    public RequestException(HttpStatusCode statusCode, string message) : base(message)
    {
        _statusCode = statusCode;
    }
    
    public RequestException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
    {
        _statusCode = statusCode;
    }

    protected RequestException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public HttpStatusCode StatusCode => _statusCode;
}