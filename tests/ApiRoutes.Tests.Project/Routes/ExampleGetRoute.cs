using ApiRoutes;

namespace Tests;

[ApiRoute("/example", Method.GET)]
public record struct ExampleGetRoute : IRequest;

public class ExampleGetRouteHandler : IHandler<ExampleGetRoute>
{
    public ValueTask<RequestResult> InvokeAsync(ExampleGetRoute request, CancellationToken cancellationToken = default)
    {
        return this.NoContentAsTask();
    }
}
