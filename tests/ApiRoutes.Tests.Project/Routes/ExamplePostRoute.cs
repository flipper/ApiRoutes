using ApiRoutes;

namespace Tests;


[ApiRoute("/example", Method.POST)]
public record struct ExamplePostRoute(string Test) : IRequest<ExamplePostRouteResponse>;

public record struct ExamplePostRouteResponse(string Test);

public class ExamplePostRouteHandler : IHandler<ExamplePostRoute, ExamplePostRouteResponse>
{
    public ValueTask<RequestResult> InvokeAsync(ExamplePostRoute request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask(new ExamplePostRouteResponse(request.Test));
    }
}
