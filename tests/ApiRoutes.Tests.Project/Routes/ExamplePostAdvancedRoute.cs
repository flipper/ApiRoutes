using ApiRoutes;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

[ApiRoute("/example/{id}", Method.POST)]
public record struct ExamplePostAdvancedRoute(string Test, [FromQuery] string Query, [FromRoute] string Id) : IRequest<ExamplePostAdvancedRouteResponse>;

public record struct ExamplePostAdvancedRouteResponse(string Test, string Query, string Id);

public class ExamplePostAdvancedRouteHandler : IHandler<ExamplePostAdvancedRoute, ExamplePostAdvancedRouteResponse>
{
    public ValueTask<RequestResult> InvokeAsync(ExamplePostAdvancedRoute request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask(new ExamplePostAdvancedRouteResponse(request.Test, request.Query, request.Id));
    }
}
