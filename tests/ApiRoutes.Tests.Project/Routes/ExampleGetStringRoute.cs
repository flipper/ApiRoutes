using ApiRoutes;

namespace Tests;

[ApiRoute("/example-string", Method.GET)]
public record struct ExampleGetStringRoute : IRequest<string>;

public class ExampleGetStringRouteHandler : IHandler<ExampleGetStringRoute, string>
{
    public ValueTask<RequestResult> InvokeAsync(ExampleGetStringRoute request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask("Hello World");
    }
}
