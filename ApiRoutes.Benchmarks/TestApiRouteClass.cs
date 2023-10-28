
namespace ApiRoutes.Benchmarks;

[ApiRoute("/test2", Method.POST)]
public class TestApiRouteRequestClass : IRequest<Response>
{
    public string Name { get; set; }
}

public class TestApiRouteHandlerClass : IHandler<TestApiRouteRequestClass, Response>
{
    public ValueTask<RequestResult> InvokeAsync(TestApiRouteRequestClass request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask(new Response
        {
            Message = $"Hello {request.Name}"
        });
    }
}