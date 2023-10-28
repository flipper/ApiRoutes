using Microsoft.AspNetCore.Mvc;

namespace ApiRoutes.Benchmarks;

[ApiRoute("/test", Method.POST)]
public struct TestApiRouteRequest : IRequest<Response>
{
    public string Name { get; set; }
    
    [FromQuery]
    public string QueryString { get; set; }
}

public class TestApiRouteHandler : IHandler<TestApiRouteRequest, Response>
{
    public ValueTask<RequestResult> InvokeAsync(TestApiRouteRequest request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask(new Response
        {
            Message = $"Hello {request.Name}",
            QueryString = request.QueryString
        });
    }
}